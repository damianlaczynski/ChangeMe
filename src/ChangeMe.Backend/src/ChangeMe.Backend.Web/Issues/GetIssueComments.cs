using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Issues;

public class GetIssueComments(IMediator mediator)
  : BaseEndpoint<GetIssueCommentsQuery, PaginationResult<IssueCommentDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/comments");
    Summary(s => s.Summary = "Get issue comments");
  }
}
