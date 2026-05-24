using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record LoginUserCommand(
  string Email,
  string Password) : ICommand<LoginResponseDto>;

public class LoginUserHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
  IOptions<AuthOptions> authOptions,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<LoginUserCommand, LoginResponseDto>
{
  public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);
    var user = await context.Users
      .Include(x => x.AccountInvitations)
      .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (user is null)
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    if (!user.IsActive)
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.DeactivatedAccountMessage);

    if (!user.HasPasswordSet)
    {
      if (ExternalAuthUtils.IsInvitationPending(user))
        return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvitePendingAccountMessage);

      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);
    }

    if (authOptions.Value.EmailVerificationEnabled && !user.EmailVerified)
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.EmailNotVerifiedMessage);

    if (!passwordHasher.VerifyPassword(user.PasswordHash, command.Password))
      return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    var utcNow = DateTime.UtcNow;
    var passwordChangeRequired = passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow);

    if (!passwordChangeRequired && twoFactorPolicyEvaluator.IsTwoFactorVerificationRequired(user))
    {
      var challengeResult = await CreateSignInChallengeAsync(user, utcNow, cancellationToken);
      if (!challengeResult.IsSuccess)
        return challengeResult.Map();

      return Result.Success(new LoginResponseDto
      {
        TwoFactorChallenge = new PendingSignInChallengeDto(challengeResult.Value.Id)
      });
    }

    var sessionResult = await AuthSessionFactory.CreateSessionAsync(
      context,
      sessionLifetime,
      httpContextAccessor,
      user,
      cancellationToken,
      SignInMethods.Password);
    if (!sessionResult.IsSuccess)
      return Result<LoginResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var passwordExpiresAtUtc = passwordExpirationEvaluator.GetPasswordExpiresAtUtc(user);
    var twoFactorSetupRequired = !passwordChangeRequired
      && twoFactorPolicyEvaluator.IsTwoFactorSetupRequired(user);
    var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == user.Id, cancellationToken);
    var passkeySetupRequired = !passwordChangeRequired
      && !twoFactorSetupRequired
      && passkeyPolicyEvaluator.IsPasskeySetupRequired(user, passkeyCount);

    var authResponse = await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      passwordChangeRequired,
      passwordExpiresAtUtc,
      twoFactorSetupRequired,
      cancellationToken,
      passkeySetupRequired);

    if (!authResponse.IsSuccess)
      return authResponse.Map();

    return Result.Success(new LoginResponseDto { AuthSession = authResponse.Value });
  }

  private async Task<Result<SignInChallenge>> CreateSignInChallengeAsync(
    User user,
    DateTime utcNow,
    CancellationToken cancellationToken)
  {
    var lifetimeMinutes = authOptions.Value.TwoFactor.PendingSignInChallengeLifetimeMinutes;
    var expiresAtUtc = utcNow.AddMinutes(lifetimeMinutes);
    var challengeResult = SignInChallenge.Create(user.Id, expiresAtUtc, SignInMethods.Password);
    if (!challengeResult.IsSuccess)
      return challengeResult.Map();

    await context.SignInChallenges
      .Where(x => x.UserId == user.Id)
      .ExecuteDeleteAsync(cancellationToken);

    await context.SignInChallenges.AddAsync(challengeResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return challengeResult;
  }
}
