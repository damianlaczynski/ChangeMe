using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserInvitationService(
  IUserAuthTokenService tokenService,
  IAuthEmailService authEmailService,
  TimeProvider timeProvider)
{
  public async Task<Result> SendInvitationAsync(User user, CancellationToken cancellationToken)
  {
    var tokenResult = await tokenService.IssueTokenAsync(
      user.Id,
      UserAuthTokenType.Invitation,
      cancellationToken);

    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    var utcNow = timeProvider.GetUtcNow().UtcDateTime;

    var recordResult = user.RecordInvitationIssued(utcNow);
    if (!recordResult.IsSuccess)
      return recordResult.Map();

    await authEmailService.SendAccountInvitationAsync(user, tokenResult.Value, cancellationToken);

    return Result.Success();
  }
}
