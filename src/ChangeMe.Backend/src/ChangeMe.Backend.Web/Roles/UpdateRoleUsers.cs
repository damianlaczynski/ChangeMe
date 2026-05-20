using ChangeMe.Backend.UseCases.Roles;

namespace ChangeMe.Backend.Web.Roles;

public class UpdateRoleUsers(IMediator mediator) : BaseEndpoint<UpdateRoleUsersCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.RolesManage);
    Put("/roles/{RoleId}/users");
    Summary(s => s.Summary = "Update role user assignments");
  }
}

public sealed class UpdateRoleUsersCommandValidator : Validator<UpdateRoleUsersCommand>
{
  public UpdateRoleUsersCommandValidator()
  {
    RuleFor(x => x.RoleId).NotEmpty();
  }
}
