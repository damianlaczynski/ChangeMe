using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetProjectById(IMediator mediator) : BaseEndpoint<GetProjectByIdQuery, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/projects/{Id}");
    Summary(s => s.Summary = "Get project by id");
  }
}

public sealed class GetProjectByIdQueryValidator : Validator<GetProjectByIdQuery>
{
  public GetProjectByIdQueryValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
