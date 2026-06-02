using System.Net.Mime;
using ChangeMe.Backend.UseCases.Issues;
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
    var result = await mediator.Send(
      new GetIssueAttachmentContentQuery(
        Route<Guid>("IssueId"),
        Route<Guid>("AttachmentId")),
      ct);

    if (!result.IsSuccess)
    {
      await HttpContext.SendResultAsync(result, ct);
      return;
    }

    var content = result.Value;
    var response = HttpContext.Response;
    response.Headers["X-Content-Type-Options"] = "nosniff";
    response.ContentType = content.ContentType;
    response.Headers.ContentDisposition = new ContentDisposition
    {
      FileName = content.OriginalFileName,
      DispositionType = DispositionTypeNames.Attachment
    }.ToString();

    await using (content.Content)
      await content.Content.CopyToAsync(response.Body, ct);
  }
}
