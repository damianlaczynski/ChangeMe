using ChangeMe.Backend.UseCases.Roles;
using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.Web.Roles;

public class GetRoles(IMediator mediator) : BaseEndpoint<GetRolesQuery, IReadOnlyList<RoleListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesView);
    Get("/roles");
    Summary(s => s.Summary = "Get roles");
  }
}
