using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetManageableProjects(IMediator mediator)
  : BaseEndpoint<GetManageableProjectsQuery, IReadOnlyList<ProjectOptionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/projects/manageable");
    Summary(s => s.Summary = "Get manageable projects for current user");
  }
}
