using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.Web.Issues;

public class GetIssueHistory(IMediator mediator)
  : BaseEndpoint<GetIssueHistoryQuery, PaginationResult<IssueHistoryEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/history");
    Summary(s => s.Summary = "Get issue history");
  }
}
