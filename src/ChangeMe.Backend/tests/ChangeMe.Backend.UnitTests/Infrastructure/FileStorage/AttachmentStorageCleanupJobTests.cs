using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Common.Attachments;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.FileStorage;

public sealed class AttachmentStorageCleanupJobTests
{
  [Fact]
  public async Task ExecuteAsync_ShouldRemoveStalePendingRowsAndOrphanedFiles()
  {
    var rootPath = Path.Combine(Path.GetTempPath(), $"changeme-cleanup-{Guid.NewGuid():N}");
    Directory.CreateDirectory(rootPath);

    var options = Options.Create(new FileStorageOptions
    {
      RootPath = rootPath,
      PendingRetentionMinutes = 30
    });

    var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero));
    await using var context = CreateInMemoryContext();

    var issueId = Guid.CreateVersion7();
    var stalePending = IssueAttachment.CreatePending(
      issueId,
      "stale.txt",
      "text/plain",
      5).Value;
    stalePending.CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddHours(-2);
    stalePending.CreatedBy = Guid.CreateVersion7();

    var activeAttachment = IssueAttachment.CreatePending(
      issueId,
      "active.txt",
      "text/plain",
      5).Value;
    activeAttachment.Activate<IssueAttachment>();
    activeAttachment.CreatedAt = timeProvider.GetUtcNow().UtcDateTime;
    activeAttachment.CreatedBy = Guid.CreateVersion7();

    context.Attachments.AddRange(stalePending, activeAttachment);
    await context.SaveChangesAsync();

    stalePending.CreatedAt = timeProvider.GetUtcNow().UtcDateTime.AddHours(-2);
    await context.SaveChangesAsync();

    var storage = new LocalFileStorageService(options);
    await using (var staleStream = new MemoryStream("stale"u8.ToArray()))
      await storage.SaveAsync(FileStorageContainers.Issues, issueId, stalePending.StorageKey, staleStream, CancellationToken.None);
    await using (var activeStream = new MemoryStream("active"u8.ToArray()))
      await storage.SaveAsync(FileStorageContainers.Issues, issueId, activeAttachment.StorageKey, activeStream, CancellationToken.None);

    var orphanOwnerId = Guid.CreateVersion7();
    var orphanKey = IssueAttachment.CreatePending(orphanOwnerId, "orphan.txt", "text/plain", 1).Value.StorageKey;
    await using (var orphanStream = new MemoryStream("orphan"u8.ToArray()))
      await storage.SaveAsync(FileStorageContainers.Issues, orphanOwnerId, orphanKey, orphanStream, CancellationToken.None);

    var job = new AttachmentStorageCleanupJob(
      context,
      storage,
      options,
      timeProvider,
      NullLogger<AttachmentStorageCleanupJob>.Instance);

    await job.ExecuteAsync(TestContext.Current.CancellationToken);

    Assert.False(await context.Attachments.AnyAsync(a => a.Id == stalePending.Id));
    Assert.True(await context.Attachments.AnyAsync(a => a.Id == activeAttachment.Id));

    var storedKeys = await storage.ListStoredFileKeysAsync(TestContext.Current.CancellationToken);
    Assert.Single(storedKeys);
    Assert.Equal(activeAttachment.StorageKey, storedKeys[0].StorageKey);

    Directory.Delete(rootPath, recursive: true);
  }

  private static ApplicationDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new ApplicationDbContext(options);
  }

  private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
  {
    public override DateTimeOffset GetUtcNow() => utcNow;
  }
}
