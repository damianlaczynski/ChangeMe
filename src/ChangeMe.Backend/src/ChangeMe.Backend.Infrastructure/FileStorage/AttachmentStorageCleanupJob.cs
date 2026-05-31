using ChangeMe.Backend.Domain.Common.Attachments;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.Infrastructure.Persistence;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class AttachmentStorageCleanupJob(
  ApplicationDbContext context,
  IFileStorageService fileStorageService,
  IOptions<FileStorageOptions> fileStorageOptions,
  TimeProvider timeProvider,
  ILogger<AttachmentStorageCleanupJob> logger)
{
  public Task ExecuteAsync(IJobCancellationToken jobCancellationToken)
  {
    jobCancellationToken.ThrowIfCancellationRequested();
    return ExecuteAsync(jobCancellationToken.ShutdownToken);
  }

  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var pendingRetentionMinutes = fileStorageOptions.Value.PendingRetentionMinutes;
    if (pendingRetentionMinutes <= 0)
      pendingRetentionMinutes = 30;

    var pendingCutoffUtc = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(-pendingRetentionMinutes);

    var stalePendingAttachments = await context.Attachments
      .Where(a => a.Status == AttachmentStatus.PENDING && a.CreatedAt < pendingCutoffUtc)
      .ToListAsync(cancellationToken);

    foreach (var attachment in stalePendingAttachments)
    {
      await fileStorageService.DeleteAsync(
        attachment.StorageContainer,
        attachment.OwnerId,
        attachment.StorageKey,
        cancellationToken);
      context.Attachments.Remove(attachment);
    }

    if (stalePendingAttachments.Count > 0)
    {
      await context.SaveChangesAsync(cancellationToken);
      logger.LogInformation(
        "Attachment cleanup removed {DeletedCount} stale pending attachment rows",
        stalePendingAttachments.Count);
    }

    var knownKeys = await context.Attachments
      .AsNoTracking()
      .Select(a => new { a.StorageContainer, a.OwnerId, a.StorageKey })
      .ToListAsync(cancellationToken);

    var knownKeySet = knownKeys
      .Select(x => (x.StorageContainer, x.OwnerId, x.StorageKey))
      .ToHashSet();

    var storedKeys = await fileStorageService.ListStoredFileKeysAsync(cancellationToken);
    var orphanedFiles = storedKeys
      .Where(key => !knownKeySet.Contains((key.Container, key.OwnerId, key.StorageKey)))
      .ToList();

    foreach (var orphanedFile in orphanedFiles)
    {
      await fileStorageService.DeleteAsync(
        orphanedFile.Container,
        orphanedFile.OwnerId,
        orphanedFile.StorageKey,
        cancellationToken);
    }

    if (orphanedFiles.Count > 0)
    {
      logger.LogInformation(
        "Attachment cleanup removed {DeletedCount} orphaned stored files",
        orphanedFiles.Count);
    }
  }
}
