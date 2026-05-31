using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Issues;

public class GetIssueAttachments(IMediator mediator)
  : BaseEndpoint<GetIssueAttachmentsQuery, PaginationResult<IssueAttachmentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/attachments");
    Summary(s => s.Summary = "Get issue attachments");
  }
}
