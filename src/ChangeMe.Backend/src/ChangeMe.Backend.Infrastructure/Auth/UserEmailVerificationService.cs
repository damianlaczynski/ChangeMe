using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserEmailVerificationService(
  ApplicationDbContext context,
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

    await context.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
