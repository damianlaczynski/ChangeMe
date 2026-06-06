using ChangeMe.Backend.UseCases.Projects;

namespace ChangeMe.Backend.Web.Projects;

public class RemoveProjectMember(IMediator mediator) : BaseEndpoint<RemoveProjectMemberCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/projects/{ProjectId}/members/{UserId}");
    Summary(s => s.Summary = "Remove project member");
  }
}

public sealed class RemoveProjectMemberCommandValidator : Validator<RemoveProjectMemberCommand>
{
  public RemoveProjectMemberCommandValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.UserId).NotEmpty();
  }
}
