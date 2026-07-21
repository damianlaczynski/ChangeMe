using ChangeMe.Backend.UseCases.Users;

namespace ChangeMe.Backend.Web.Users;

public class RevokeAllUserSessions(IMediator mediator) : BaseEndpoint<RevokeAllUserSessionsCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsManageAny);
    Post("/users/{Id}/sessions/revoke-all");
    Summary(s => s.Summary = "Revoke all user sessions");
  }
}
