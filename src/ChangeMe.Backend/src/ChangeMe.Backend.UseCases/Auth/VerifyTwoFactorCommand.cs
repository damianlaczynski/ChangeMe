using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record VerifyTwoFactorCommand(
  Guid ChallengeId,
  string VerificationCode) : ICommand<AuthResponseDto>;

public class VerifyTwoFactorHandler(
  ApplicationDbContext context,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IOptions<AuthOptions> authOptions,
  IHttpContextAccessor httpContextAccessor) : ICommandHandler<VerifyTwoFactorCommand, AuthResponseDto>
{
  public async Task<Result<AuthResponseDto>> Handle(VerifyTwoFactorCommand command, CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var maxAttempts = authOptions.Value.TwoFactor.MaxFailedVerificationAttempts;

    var challenge = await context.SignInChallenges
      .FirstOrDefaultAsync(x => x.Id == command.ChallengeId, cancellationToken);
    if (challenge is null || challenge.IsExpired(utcNow))
      return Result<AuthResponseDto>.Unauthorized(TwoFactorAuthUtils.SignInTimedOutMessage);

    if (challenge.FailedAttemptCount >= maxAttempts)
      return Result<AuthResponseDto>.Unauthorized(TwoFactorAuthUtils.TooManyAttemptsMessage);

    var user = await context.Users
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == challenge.UserId, cancellationToken);
    if (user is null || !user.IsActive || !user.TwoFactorEnabled)
      return Result<AuthResponseDto>.Unauthorized(TwoFactorAuthUtils.SignInTimedOutMessage);

    if (!TwoFactorAuthUtils.TryValidateVerificationCode(
          user,
          command.VerificationCode,
          totpService,
          secretProtector,
          recoveryCodeHasher,
          authOptions,
          utcNow,
          out var consumedRecoveryCode))
    {
      challenge.RecordFailedAttempt();
      if (challenge.FailedAttemptCount >= maxAttempts)
      {
        await context.SignInChallenges
          .Where(x => x.Id == challenge.Id)
          .ExecuteDeleteAsync(cancellationToken);
        return Result<AuthResponseDto>.Unauthorized(TwoFactorAuthUtils.TooManyAttemptsMessage);
      }

      await context.SaveChangesAsync(cancellationToken);
      return Result<AuthResponseDto>.Unauthorized(TwoFactorAuthUtils.InvalidVerificationCodeMessage);
    }

    if (consumedRecoveryCode is not null)
    {
      consumedRecoveryCode.MarkUsed(utcNow);
      await authEmailService.SendRecoveryCodeUsedAsync(user, cancellationToken);
    }

    await context.SignInChallenges
      .Where(x => x.Id == challenge.Id)
      .ExecuteDeleteAsync(cancellationToken);

    var pendingSignInMethod = string.IsNullOrWhiteSpace(challenge.PendingSignInMethod)
      ? SignInMethods.Password
      : challenge.PendingSignInMethod!;

    var sessionResult = await AuthSessionFactory.CreateSessionAsync(
      context,
      sessionLifetime,
      httpContextAccessor,
      user,
      cancellationToken,
      pendingSignInMethod);
    if (!sessionResult.IsSuccess)
      return Result<AuthResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var passwordChangeRequired = passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow);
    var passwordExpiresAtUtc = passwordExpirationEvaluator.GetPasswordExpiresAtUtc(user);
    var twoFactorSetupRequired = !passwordChangeRequired
      && twoFactorPolicyEvaluator.IsTwoFactorSetupRequired(user);
    var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == user.Id, cancellationToken);
    var passkeySetupRequired = !passwordChangeRequired
      && !twoFactorSetupRequired
      && passkeyPolicyEvaluator.IsPasskeySetupRequired(user, passkeyCount);

    return await AuthSessionUtils.CreateAuthResponseAsync(
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
  }
}
