using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class GetRolesForAssignment(IMediator mediator)
  : BaseEndpoint<GetRolesForAssignmentQuery, IReadOnlyList<RoleAssignmentOptionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Get("/users/roles");
    Summary(s => s.Summary = "Get roles for user assignment");
  }
}
