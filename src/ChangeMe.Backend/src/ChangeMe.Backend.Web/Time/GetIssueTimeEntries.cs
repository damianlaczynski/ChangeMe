using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetIssueTimeEntries(IMediator mediator)
  : BaseEndpoint<GetIssueTimeEntriesQuery, IssueTimeEntriesResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/issues/{IssueId}/time-entries");
    Summary(s => s.Summary = "Get issue time entries");
  }
}

public sealed class GetIssueTimeEntriesQueryValidator : Validator<GetIssueTimeEntriesQuery>
{
  public GetIssueTimeEntriesQueryValidator()
  {
    RuleFor(x => x.IssueId).NotEmpty();
  }
}
