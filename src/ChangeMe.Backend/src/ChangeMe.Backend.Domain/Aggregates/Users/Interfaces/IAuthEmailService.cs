namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IAuthEmailService
{
  Task<Result> SendAccountInvitationAsync(User user, string plainToken, CancellationToken cancellationToken = default);

  Task<Result> SendPasswordResetRequestedAsync(User user, string plainToken, CancellationToken cancellationToken = default);

  Task<Result> SendPasswordResetCompletedAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendPasswordChangedAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendVerifyEmailAsync(User user, string plainToken, CancellationToken cancellationToken = default);

  Task<Result> SendTwoFactorEnabledAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendTwoFactorDisabledAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendTwoFactorResetByAdminAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendRecoveryCodeUsedAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendExternalAccountLinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default);

  Task<Result> SendExternalAccountUnlinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default);

  Task<Result> SendPasskeyAddedAsync(User user, string passkeyName, CancellationToken cancellationToken = default);

  Task<Result> SendPasskeyRemovedAsync(User user, string passkeyName, CancellationToken cancellationToken = default);

  Task<Result> SendPasskeysResetByAdminAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendEmailChangeRequestedAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendConfirmEmailChangeAsync(string newEmail, string plainToken, CancellationToken cancellationToken = default);

  Task<Result> SendEmailChangeCancelledAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendEmailChangeCompletedAsync(
    string previousEmail,
    string newEmail,
    CancellationToken cancellationToken = default);

  Task<Result> SendEmailChangedByAdminAsync(
    string previousEmail,
    string newEmail,
    CancellationToken cancellationToken = default);
}
