using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class GetMySessions(IMediator mediator) : BaseEndpoint<GetMySessionsQuery, PaginationResult<UserSessionDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.SessionsViewOwn);
    Get("/auth/sessions");
    Summary(s =>
    {
      s.Summary = "List my sessions";
      s.Description = "Returns active sessions for the signed-in user.";
    });
  }
}
