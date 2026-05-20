using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.Web.Roles;

public class GetRoleForm(IMediator mediator) : BaseEndpoint<GetRoleFormQuery, RoleFormDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Get("/roles/{Id}/form");
    Summary(s => s.Summary = "Get role form");
  }
}
