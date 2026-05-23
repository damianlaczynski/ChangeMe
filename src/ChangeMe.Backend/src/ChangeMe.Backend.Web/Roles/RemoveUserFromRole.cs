using ChangeMe.Backend.UseCases.Roles;

namespace ChangeMe.Backend.Web.Roles;

public class RemoveUserFromRole(IMediator mediator) : BaseEndpoint<RemoveUserFromRoleCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Delete("/roles/{RoleId}/users/{UserId}");
    Summary(s => s.Summary = "Remove user from role");
  }
}
