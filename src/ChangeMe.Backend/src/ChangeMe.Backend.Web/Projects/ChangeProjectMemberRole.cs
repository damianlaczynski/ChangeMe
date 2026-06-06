using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.UseCases.Projects;

namespace ChangeMe.Backend.Web.Projects;

public class ChangeProjectMemberRole(IMediator mediator) : BaseEndpoint<ChangeProjectMemberRoleCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/projects/{ProjectId}/members/{UserId}/role");
    Summary(s => s.Summary = "Change project member role");
  }
}

public sealed class ChangeProjectMemberRoleCommandValidator : Validator<ChangeProjectMemberRoleCommand>
{
  public ChangeProjectMemberRoleCommandValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Role).IsInEnum();
  }
}
