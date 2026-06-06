using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetTimeSettings(IMediator mediator)
  : BaseEndpointWithoutRequest<GetTimeSettingsQuery, TimeSettingsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/settings");
    RequirePermission(PermissionCodes.TimeViewReports);
    Summary(s => s.Summary = "Get time tracking settings");
  }
}
