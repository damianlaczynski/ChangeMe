using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.Web.Roles;

public class GetManageRoleUsersForm(IMediator mediator)
  : BaseEndpoint<GetManageRoleUsersFormQuery, ManageRoleUsersFormDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Get("/roles/{Id}/manage-users/form");
    Summary(s => s.Summary = "Get manage role users form");
  }
}
