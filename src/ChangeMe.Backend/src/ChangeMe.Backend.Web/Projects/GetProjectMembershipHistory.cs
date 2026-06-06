using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetProjectMembershipHistory(IMediator mediator)
  : BaseEndpoint<GetProjectMembershipHistoryQuery, PaginationResult<ProjectMembershipHistoryEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/projects/{ProjectId}/membership-history");
    Summary(s => s.Summary = "Get project membership history");
  }
}

public sealed class GetProjectMembershipHistoryQueryValidator : Validator<GetProjectMembershipHistoryQuery>
{
  public GetProjectMembershipHistoryQueryValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
  }
}
