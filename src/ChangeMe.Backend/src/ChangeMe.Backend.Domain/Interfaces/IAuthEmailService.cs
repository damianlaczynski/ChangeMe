using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Domain.Interfaces;

public interface IAuthEmailService
{
  Task<Result> SendAccountInvitationAsync(User user, string plainToken, CancellationToken cancellationToken = default);

  Task<Result> SendPasswordResetRequestedAsync(User user, string plainToken, CancellationToken cancellationToken = default);

  Task<Result> SendPasswordResetCompletedAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendPasswordChangedAsync(User user, CancellationToken cancellationToken = default);

  Task<Result> SendVerifyEmailAsync(User user, string plainToken, CancellationToken cancellationToken = default);
}
