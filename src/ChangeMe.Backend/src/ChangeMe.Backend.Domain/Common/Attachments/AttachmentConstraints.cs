namespace ChangeMe.Backend.Domain.Common.Attachments;

public static class AttachmentConstraints
{
  public const int ORIGINAL_FILE_NAME_MAX_LENGTH = 255;
  public const int CONTENT_TYPE_MAX_LENGTH = 100;
  public const int STORAGE_KEY_MAX_LENGTH = 64;
  public const int STORAGE_CONTAINER_MAX_LENGTH = 64;
  public const int MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024;
  public const int MAX_ATTACHMENTS_PER_ISSUE = 10;
}
