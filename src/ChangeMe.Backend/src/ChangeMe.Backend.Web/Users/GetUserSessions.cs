using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class GetUserSessions(IMediator mediator)
  : BaseEndpoint<GetUserSessionsQuery, PaginationResult<AdminUserSessionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsViewAny);
    Get("/users/{Id}/sessions");
    Summary(s => s.Summary = "Get user active sessions");
  }
}
