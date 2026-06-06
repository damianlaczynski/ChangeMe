using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetProjects(IMediator mediator) : BaseEndpoint<GetProjectsQuery, PaginationResult<ProjectListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/projects");
    Summary(s => s.Summary = "Get projects");
  }
}
