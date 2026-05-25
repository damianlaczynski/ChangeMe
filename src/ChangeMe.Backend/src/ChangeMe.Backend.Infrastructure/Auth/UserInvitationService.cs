using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserInvitationService(
  ApplicationDbContext context,
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

  public async Task<Result> CancelInvitationAsync(User user, CancellationToken cancellationToken)
  {
    var utcNow = timeProvider.GetUtcNow().UtcDateTime;

    var cancelResult = user.CancelPendingInvitations(utcNow);
    if (!cancelResult.IsSuccess)
      return cancelResult;

    await context.SaveChangesAsync(cancellationToken);

    await tokenService.InvalidateUnusedTokensAsync(
      user.Id,
      UserAuthTokenType.Invitation,
      cancellationToken);

    return Result.Success();
  }
}
