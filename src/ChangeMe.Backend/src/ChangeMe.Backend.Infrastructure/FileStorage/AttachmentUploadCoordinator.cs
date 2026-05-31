using ChangeMe.Backend.Domain.Common.Attachments;
using ChangeMe.Backend.Infrastructure.Persistence;

namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class AttachmentUploadCoordinator(
  ApplicationDbContext context,
  IFileContentValidator fileContentValidator,
  IFileStorageService fileStorageService)
{
  public Result<FileContentValidationResult> ValidateUpload(
    string originalFileName,
    string? declaredContentType,
    ReadOnlySpan<byte> contentPreview,
    long sizeBytes) =>
    fileContentValidator.Validate(originalFileName, declaredContentType, contentPreview, sizeBytes);

  public async Task<Result> ReservePendingAsync(Attachment attachment, CancellationToken cancellationToken)
  {
    context.Attachments.Add(attachment);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }

  public Task<Result> WriteContentAsync(
    Attachment attachment,
    Stream content,
    CancellationToken cancellationToken) =>
    fileStorageService.SaveAsync(
      attachment.StorageContainer,
      attachment.OwnerId,
      attachment.StorageKey,
      content,
      cancellationToken);

  public async Task RollbackPendingAsync(Attachment attachment, CancellationToken cancellationToken)
  {
    context.Attachments.Remove(attachment);
    await context.SaveChangesAsync(cancellationToken);
    await fileStorageService.DeleteAsync(
      attachment.StorageContainer,
      attachment.OwnerId,
      attachment.StorageKey,
      cancellationToken);
  }
}
