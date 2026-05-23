using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserInvitationService(
  IUserAuthTokenService tokenService,
  IAuthEmailService authEmailService)
{
  public async Task<Result> SendInvitationAsync(User user, CancellationToken cancellationToken)
  {
    var tokenResult = await tokenService.IssueTokenAsync(
      user.Id,
      UserAuthTokenType.Invitation,
      cancellationToken);

    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    user.RecordInvitationSent();
    await authEmailService.SendAccountInvitationAsync(user, tokenResult.Value, cancellationToken);

    return Result.Success();
  }
}
