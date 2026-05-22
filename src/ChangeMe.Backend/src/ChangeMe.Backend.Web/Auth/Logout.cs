using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.Web.Auth;

public class Logout(IMediator mediator) : BaseEndpoint<LogoutCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/logout");
    Summary(s =>
    {
      s.Summary = "Logout";
      s.Description = "Revoke the current session.";
    });
  }
}
