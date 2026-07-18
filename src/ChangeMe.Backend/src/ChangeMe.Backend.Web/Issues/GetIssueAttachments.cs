using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using QueryGrid.Abstractions;

namespace ChangeMe.Backend.Web.Issues;

public class GetIssueAttachments(IMediator mediator)
  : BaseEndpoint<GetIssueAttachmentsQuery, GridResult<IssueAttachmentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.IssuesView);
    Get("/issues/{IssueId}/attachments");
    Summary(s => s.Summary = "Get issue attachments");
  }
}
