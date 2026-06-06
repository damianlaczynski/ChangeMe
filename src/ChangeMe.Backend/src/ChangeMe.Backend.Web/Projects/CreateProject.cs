using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class CreateProject(IMediator mediator) : BaseEndpoint<CreateProjectCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/projects");
    Summary(s => s.Summary = "Create project");
  }
}

public sealed class CreateProjectCommandValidator : Validator<CreateProjectCommand>
{
  public CreateProjectCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MinimumLength(ProjectConstraints.NAME_MIN_LENGTH)
      .MaximumLength(ProjectConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Description)
      .MaximumLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));
  }
}
