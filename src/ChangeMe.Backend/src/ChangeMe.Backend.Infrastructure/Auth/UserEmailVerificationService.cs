using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;

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
      cancellationToken: cancellationToken);

    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    var emailResult = await authEmailService.SendVerifyEmailAsync(
      user,
      tokenResult.Value,
      cancellationToken);

    if (!emailResult.IsSuccess)
      return emailResult;

    return Result.Success();
  }
}
