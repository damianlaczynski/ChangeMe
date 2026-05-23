using ChangeMe.Backend.UseCases.Users;

namespace ChangeMe.Backend.Web.Users;

public class RevokeUserSession(IMediator mediator) : BaseEndpoint<RevokeUserSessionCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsManageAny);
    Delete("/users/{Id}/sessions/{SessionId}");
    Summary(s => s.Summary = "Revoke user session");
  }
}
