using System.Text;
using ChangeMe.Backend.Domain.Common.Attachments;
using Microsoft.Extensions.Options;
using MimeDetective;

namespace ChangeMe.Backend.Infrastructure.FileStorage;

public sealed class FileContentValidator(
  IOptions<FileStorageOptions> options,
  FileContentInspectorProvider inspectorProvider) : IFileContentValidator
{
  public Result<FileContentValidationResult> Validate(
    string originalFileName,
    string? declaredContentType,
    ReadOnlySpan<byte> contentPreview,
    long sizeBytes)
  {
    var maxFileSizeBytes = options.Value.MaxFileSizeBytes;
    if (sizeBytes <= 0)
      return Result<FileContentValidationResult>.Invalid([new ValidationError("File", "cannot be empty")]);

    if (sizeBytes > maxFileSizeBytes)
      return Result<FileContentValidationResult>.Invalid([new ValidationError("File", $"cannot exceed {maxFileSizeBytes} bytes")]);

    var sanitizedFileName = SanitizeFileName(originalFileName);
    if (string.IsNullOrWhiteSpace(sanitizedFileName))
      return Result<FileContentValidationResult>.Invalid([new ValidationError("File", "file name is invalid")]);

    var extension = Path.GetExtension(sanitizedFileName);
    if (string.IsNullOrWhiteSpace(extension))
      return Result<FileContentValidationResult>.Invalid([new ValidationError("File", "file extension is required")]);

    extension = extension.ToLowerInvariant();
    if (!IsExtensionAllowed(extension))
      return Result<FileContentValidationResult>.Invalid([new ValidationError("File", "file type is not allowed")]);

    var profile = FileExtensionProfiles.GetProfile(extension);
    if (profile is null)
      return Result<FileContentValidationResult>.Invalid([new ValidationError("File", "file type is not allowed")]);

    if (!ContentMatchesExtension(profile, contentPreview))
      return Result<FileContentValidationResult>.Invalid([
        new ValidationError("File", "file content does not match file extension")
      ]);

    if (!string.IsNullOrWhiteSpace(declaredContentType)
        && !string.Equals(
          declaredContentType.Split(';')[0].Trim(),
          profile.ContentType,
          StringComparison.OrdinalIgnoreCase))
      return Result<FileContentValidationResult>.Invalid([
        new ValidationError("File", "declared content type does not match file content")
      ]);

    return Result.Success(new FileContentValidationResult(
      sanitizedFileName,
      profile.ContentType,
      extension));
  }

  private bool ContentMatchesExtension(
    FileExtensionProfile profile,
    ReadOnlySpan<byte> content)
  {
    var inspectionResults = inspectorProvider.Inspector
      .Inspect(content.ToArray())
      .Where(match => match.Definition?.File?.Extensions is { Length: > 0 })
      .OrderByDescending(match => match.Points)
      .ToList();

    var allowedMatches = inspectionResults
      .Where(match => match.Definition!.File.Extensions
        .Any(extension => FileExtensionProfiles.IsConfiguredExtension($".{extension}")))
      .ToList();

    if (profile.RequiresBinarySignature)
    {
      return allowedMatches.Any(match =>
        profile.IsCompatibleWithDetectedExtensions(match.Definition!.File.Extensions));
    }

    var conflictingBinaryMatches = allowedMatches
      .Where(match =>
      {
        var detectedProfile = match.Definition!.File.Extensions
          .Select(ext => FileExtensionProfiles.GetProfile($".{ext}"))
          .FirstOrDefault(candidate => candidate is not null);

        return detectedProfile is { RequiresBinarySignature: true }
          && !profile.IsCompatibleWithDetectedExtensions(match.Definition.File.Extensions);
      })
      .ToList();

    if (conflictingBinaryMatches.Count > 0)
      return false;

    return IsTextContent(content);
  }

  private bool IsExtensionAllowed(string extension) =>
    options.Value.AllowedExtensions.Any(x => string.Equals(x, extension, StringComparison.OrdinalIgnoreCase))
    && FileExtensionProfiles.IsConfiguredExtension(extension);

  private static string SanitizeFileName(string originalFileName)
  {
    var fileName = Path.GetFileName(originalFileName).Trim();
    if (fileName.Length == 0)
      return string.Empty;

    var invalidChars = Path.GetInvalidFileNameChars();
    var builder = new StringBuilder(fileName.Length);

    foreach (var character in fileName)
    {
      if (invalidChars.Contains(character) || char.IsControl(character))
        continue;

      builder.Append(character);
    }

    var sanitized = builder.ToString().Trim();
    if (sanitized.Length > AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH)
      sanitized = sanitized[..AttachmentConstraints.ORIGINAL_FILE_NAME_MAX_LENGTH];

    return sanitized;
  }

  private static bool IsTextContent(ReadOnlySpan<byte> contentPreview)
  {
    if (contentPreview.IsEmpty)
      return true;

    foreach (var value in contentPreview)
    {
      if (value == 0)
        return false;
    }

    return true;
  }
}
