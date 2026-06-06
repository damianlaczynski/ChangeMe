using ChangeMe.Backend.UseCases.Projects;

namespace ChangeMe.Backend.Web.Projects;

public class DeleteProject(IMediator mediator) : BaseEndpoint<DeleteProjectCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/projects/{Id}");
    Summary(s => s.Summary = "Delete project");
  }
}

public sealed class DeleteProjectCommandValidator : Validator<DeleteProjectCommand>
{
  public DeleteProjectCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
