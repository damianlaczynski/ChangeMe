using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetProjectOperationHistory(IMediator mediator)
  : BaseEndpoint<GetProjectOperationHistoryQuery, PaginationResult<ProjectOperationHistoryEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/projects/{ProjectId}/operation-history");
    Summary(s => s.Summary = "Get project operation history");
  }
}

public sealed class GetProjectOperationHistoryQueryValidator : Validator<GetProjectOperationHistoryQuery>
{
  public GetProjectOperationHistoryQueryValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
  }
}
