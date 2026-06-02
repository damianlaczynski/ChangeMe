namespace ChangeMe.Backend.Infrastructure.FileStorage;

using ChangeMe.Backend.Domain.Common.Attachments;

public class FileStorageOptions
{
  public const string SectionName = "FileStorage";

  public string RootPath { get; set; } = "../../storage";

  public int MaxFileSizeBytes { get; set; } = AttachmentConstraints.MAX_FILE_SIZE_BYTES;

  public string[] AllowedExtensions { get; set; } =
  [
    ".pdf",
    ".png",
    ".jpg",
    ".jpeg",
    ".gif",
    ".txt",
    ".csv",
    ".docx",
    ".xlsx"
  ];

  public int PendingRetentionMinutes { get; set; } = 30;

  public string CleanupCronExpression { get; set; } = "0 * * * *";
}
