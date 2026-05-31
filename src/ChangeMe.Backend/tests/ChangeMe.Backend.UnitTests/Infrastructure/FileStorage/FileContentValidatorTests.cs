using ChangeMe.Backend.Domain.Common.Attachments;
using ChangeMe.Backend.Infrastructure.FileStorage;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.FileStorage;

public sealed class FileContentValidatorTests
{
  private readonly FileContentValidator validator = new(
    Options.Create(new FileStorageOptions()),
    new FileContentInspectorProvider());

  [Fact]
  public void Validate_WhenPdfHasValidMagicBytes_ShouldSucceed()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();

    var result = validator.Validate("report.pdf", "application/pdf", content, content.LongLength);

    Assert.True(result.IsSuccess);
    Assert.Equal("application/pdf", result.Value.ContentType);
    Assert.Equal("report.pdf", result.Value.SanitizedFileName);
  }

  [Fact]
  public void Validate_WhenPdfExtensionHasTextContent_ShouldFail()
  {
    var content = "plain text"u8.ToArray();

    var result = validator.Validate("report.pdf", "application/pdf", content, content.LongLength);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void Validate_WhenFileExceedsConfiguredLimit_ShouldFail()
  {
    var result = validator.Validate(
      "notes.txt",
      "text/plain",
      "hello"u8.ToArray(),
      AttachmentConstraints.MAX_FILE_SIZE_BYTES + 1);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void Validate_WhenFileNameContainsPathSegments_ShouldSanitizeToLeafName()
  {
    var content = "hello"u8.ToArray();

    var result = validator.Validate("../../notes.txt", "text/plain", content, content.LongLength);

    Assert.True(result.IsSuccess);
    Assert.Equal("notes.txt", result.Value.SanitizedFileName);
  }

  [Fact]
  public void Validate_WhenTxtExtensionContainsPdfContent_ShouldFail()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();

    var result = validator.Validate("notes.txt", "text/plain", content, content.LongLength);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void Validate_WhenDeclaredContentTypeDoesNotMatchProfile_ShouldFail()
  {
    var content = "%PDF-1.7 sample"u8.ToArray();

    var result = validator.Validate("report.pdf", "text/plain", content, content.LongLength);

    Assert.False(result.IsSuccess);
  }
}
