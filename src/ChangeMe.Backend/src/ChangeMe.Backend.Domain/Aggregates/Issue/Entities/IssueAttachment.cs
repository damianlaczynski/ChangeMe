using ChangeMe.Backend.Domain.Common.Attachments;

namespace ChangeMe.Backend.Domain.Aggregates.Issue.Entities;

public sealed class IssueAttachment : Attachment
{
  private IssueAttachment() { }

  public static Result<IssueAttachment> Create(
    Guid issueId,
    string originalFileName,
    string contentType,
    long sizeBytes)
  {
    var validationErrors = ValidateMetadata(
      IssueConstraints.STORAGE_CONTAINER,
      issueId,
      originalFileName,
      contentType,
      sizeBytes);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new IssueAttachment
    {
      StorageContainer = IssueConstraints.STORAGE_CONTAINER,
      OwnerId = issueId,
      OriginalFileName = originalFileName.Trim(),
      ContentType = contentType.Trim(),
      SizeBytes = sizeBytes,
      StorageKey = GenerateStorageKey(),
    });
  }
}
