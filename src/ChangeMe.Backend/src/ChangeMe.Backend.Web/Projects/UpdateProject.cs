using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class UpdateProject(IMediator mediator) : BaseEndpoint<UpdateProjectCommand, ProjectDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/projects/{Id}");
    Summary(s => s.Summary = "Update project");
  }
}

public sealed class UpdateProjectCommandValidator : Validator<UpdateProjectCommand>
{
  public UpdateProjectCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
    RuleFor(x => x.Name)
      .NotEmpty()
      .MinimumLength(ProjectConstraints.NAME_MIN_LENGTH)
      .MaximumLength(ProjectConstraints.NAME_MAX_LENGTH);

    RuleFor(x => x.Description)
      .MaximumLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));
  }
}
