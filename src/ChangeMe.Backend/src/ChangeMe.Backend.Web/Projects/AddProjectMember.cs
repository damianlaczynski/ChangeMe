using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.UseCases.Projects;

namespace ChangeMe.Backend.Web.Projects;

public class AddProjectMember(IMediator mediator) : BaseEndpoint<AddProjectMemberCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/projects/{ProjectId}/members");
    Summary(s => s.Summary = "Add project member");
  }
}

public sealed class AddProjectMemberCommandValidator : Validator<AddProjectMemberCommand>
{
  public AddProjectMemberCommandValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Role).IsInEnum();
  }
}
