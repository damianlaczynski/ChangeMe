using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.Web.Roles;

public class GetRoleAssignedUsers(IMediator mediator)
  : BaseEndpoint<GetRoleAssignedUsersQuery, PaginationResult<RoleAssignedUserDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles/{RoleId}/users");
    Summary(s => s.Summary = "Get users assigned to role");
  }
}
