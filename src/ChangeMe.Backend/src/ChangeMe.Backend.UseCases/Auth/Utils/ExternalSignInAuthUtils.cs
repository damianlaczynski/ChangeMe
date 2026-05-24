using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class ExternalSignInAuthUtils
{
  public static async Task<Result<ExternalSignInResponseDto>> IssueSignInResponseAsync(
    ApplicationDbContext context,
    IJwtTokenGenerator jwtTokenGenerator,
    ISessionLifetimeService sessionLifetime,
    IPasswordExpirationEvaluator passwordExpirationEvaluator,
    IHttpContextAccessor httpContextAccessor,
    AuthOptions auth,
    User user,
    bool identityProviderMfaAsserted,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var passwordChangeRequired = passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow);

    if (!passwordChangeRequired
        && ExternalAuthUtils.IsTwoFactorVerificationRequired(user, auth, identityProviderMfaAsserted))
    {
      var challengeResult = await CreateSignInChallengeAsync(
        context,
        auth,
        user,
        utcNow,
        cancellationToken);
      if (!challengeResult.IsSuccess)
        return challengeResult.Map();

      return Result.Success(new ExternalSignInResponseDto
      {
        TwoFactorChallenge = new PendingSignInChallengeDto(challengeResult.Value.Id)
      });
    }

    var sessionResult = await AuthSessionFactory.CreateSessionAsync(
      context,
      sessionLifetime,
      httpContextAccessor,
      user,
      cancellationToken);
    if (!sessionResult.IsSuccess)
      return Result<ExternalSignInResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var passwordExpiresAtUtc = passwordExpirationEvaluator.GetPasswordExpiresAtUtc(user);
    var twoFactorSetupRequired = !passwordChangeRequired
      && ExternalAuthUtils.IsTwoFactorSetupRequired(user, auth, identityProviderMfaAsserted);

    var authResponse = await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      passwordChangeRequired,
      passwordExpiresAtUtc,
      twoFactorSetupRequired,
      cancellationToken);

    if (!authResponse.IsSuccess)
      return authResponse.Map();

    return Result.Success(new ExternalSignInResponseDto { AuthSession = authResponse.Value });
  }

  private static async Task<Result<SignInChallenge>> CreateSignInChallengeAsync(
    ApplicationDbContext context,
    AuthOptions auth,
    User user,
    DateTime utcNow,
    CancellationToken cancellationToken)
  {
    var expiresAtUtc = utcNow.AddMinutes(auth.TwoFactor.PendingSignInChallengeLifetimeMinutes);
    var challengeResult = SignInChallenge.Create(user.Id, expiresAtUtc);
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
