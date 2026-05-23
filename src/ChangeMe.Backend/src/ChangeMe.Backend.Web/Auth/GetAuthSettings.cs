using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class GetAuthSettings(IMediator mediator) : BaseEndpoint<GetAuthSettingsQuery, AuthSettingsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/auth/settings");
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Get auth settings";
      s.Description = "Returns deployment auth policy flags and password rules for client forms.";
    });
  }
}
