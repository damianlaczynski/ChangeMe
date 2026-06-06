using ChangeMe.Backend.UseCases.Projects;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.Web.Projects;

public class GetProjectMembers(IMediator mediator) : BaseEndpoint<GetProjectMembersQuery, PaginationResult<ProjectMemberDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/projects/{ProjectId}/members");
    Summary(s => s.Summary = "Get project members");
  }
}

public sealed class GetProjectMembersQueryValidator : Validator<GetProjectMembersQuery>
{
  public GetProjectMembersQueryValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
  }
}
