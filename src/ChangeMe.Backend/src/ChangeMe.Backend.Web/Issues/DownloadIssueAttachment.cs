using System.Net.Mime;
using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChangeMe.Backend.Web.Issues;

public class DownloadIssueAttachment(IMediator mediator) : EndpointWithoutRequest
{
  public override void Configure()
  {
    Get("/issues/{IssueId}/attachments/{AttachmentId}/content");
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Summary(s => s.Summary = "Download issue attachment content");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var issueId = Route<Guid>("IssueId");
    var attachmentId = Route<Guid>("AttachmentId");

    var result = await mediator.Send(new GetIssueAttachmentContentQuery(issueId, attachmentId), ct);

    if (!result.IsSuccess)
    {
      var statusCode = result.Status switch
      {
        ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
        ResultStatus.NotFound => StatusCodes.Status404NotFound,
        ResultStatus.Invalid => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest
      };

      await HttpContext.Response.SendAsync(
        Result<IssueAttachmentContentDto>.Error(string.Join(' ', result.Errors)),
        statusCode,
        cancellation: ct);
      return;
    }

    var response = HttpContext.Response;
    response.Headers["X-Content-Type-Options"] = "nosniff";
    response.ContentType = result.Value.ContentType;
    response.Headers.ContentDisposition = new ContentDisposition
    {
      FileName = result.Value.OriginalFileName,
      DispositionType = DispositionTypeNames.Attachment
    }.ToString();

    await using (result.Value.Content)
    {
      await result.Value.Content.CopyToAsync(response.Body, ct);
    }
  }
}
