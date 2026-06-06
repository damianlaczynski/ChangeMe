using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Time;

namespace ChangeMe.Backend.Web.Time;

public class GetLoggableProjects(IMediator mediator)
  : BaseEndpointWithoutRequest<GetLoggableProjectsQuery, IReadOnlyList<ProjectOptionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/loggable-projects");
    Summary(s => s.Summary = "Get projects where the user can log time");
  }
}
