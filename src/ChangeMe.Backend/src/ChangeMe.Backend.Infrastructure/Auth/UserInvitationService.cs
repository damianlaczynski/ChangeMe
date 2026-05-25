using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class UserInvitationService(
  IUserAuthTokenService tokenService,
  IAuthEmailService authEmailService,
  IOptions<AuthOptions> authOptions,
  TimeProvider timeProvider)
{
  public async Task<Result> SendInvitationAsync(User user, CancellationToken cancellationToken)
  {
    var issuedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
    var linkExpiresAtUtc = issuedAtUtc.AddHours(
      authOptions.Value.Invitations.InvitationLinkLifetimeHours);

    var tokenResult = await tokenService.IssueTokenAsync(
      user.Id,
      UserAuthTokenType.Invitation,
      issuedAtUtc,
      cancellationToken);

    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    var emailResult = await authEmailService.SendAccountInvitationAsync(
      user,
      tokenResult.Value,
      cancellationToken);

    if (!emailResult.IsSuccess)
      return emailResult;

    var recordResult = user.RecordInvitationIssued(issuedAtUtc, linkExpiresAtUtc);
    if (!recordResult.IsSuccess)
      return recordResult.Map();

    return Result.Success();
  }

  public async Task<Result> CancelInvitationAsync(User user, CancellationToken cancellationToken)
  {
    var utcNow = timeProvider.GetUtcNow().UtcDateTime;

    var cancelResult = user.CancelPendingInvitations(utcNow);
    if (!cancelResult.IsSuccess)
      return cancelResult;

    await tokenService.InvalidateUnusedTokensAsync(
      user.Id,
      UserAuthTokenType.Invitation,
      cancellationToken);

    return Result.Success();
  }
}
