using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Users.Services;

public sealed class InvitationRetentionCleanupJob(
  ApplicationDbContext context,
  IOptions<AuthOptions> authOptions,
  TimeProvider timeProvider,
  ILogger<InvitationRetentionCleanupJob> logger)
{
  public Task ExecuteAsync(IJobCancellationToken jobCancellationToken)
  {
    jobCancellationToken.ThrowIfCancellationRequested();
    return ExecuteAsync(jobCancellationToken.ShutdownToken);
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var retentionDays = authOptions.Value.Invitations.Retention.RevokedInvitationRetentionDays;
    if (retentionDays <= 0)
      return;

    var cutoffUtc = timeProvider.GetUtcNow().UtcDateTime.AddDays(-retentionDays);

    var deletedCount = await context.Set<AccountInvitation>()
      .Where(x => x.RevokedAtUtc != null && x.RevokedAtUtc <= cutoffUtc)
      .ExecuteDeleteAsync(cancellationToken);

    logger.LogInformation(
      "Invitation retention cleanup removed {DeletedCount} revoked invitation rows",
      deletedCount);
  }
}
