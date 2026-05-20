using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class PreviewEffectivePermissions(IMediator mediator)
  : BaseEndpoint<PreviewEffectivePermissionsCommand, IReadOnlyList<EffectivePermissionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/effective-permissions/preview");
    Summary(s => s.Summary = "Preview effective permissions for selected roles");
  }
}
