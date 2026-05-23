using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserEmailVerificationService(
  IUserAuthTokenService tokenService,
  IAuthEmailService authEmailService)
{
  public async Task<Result> SendVerificationAsync(User user, CancellationToken cancellationToken)
  {
    var tokenResult = await tokenService.IssueTokenAsync(
      user.Id,
      UserAuthTokenType.EmailVerification,
      cancellationToken);

    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    return await authEmailService.SendVerifyEmailAsync(user, tokenResult.Value, cancellationToken);
  }
}
