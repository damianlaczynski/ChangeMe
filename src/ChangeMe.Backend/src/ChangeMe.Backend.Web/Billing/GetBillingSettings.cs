using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetBillingSettings(IMediator mediator)
  : BaseEndpointWithoutRequest<GetBillingSettingsQuery, BillingSettingsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/settings");
    RequirePermission(PermissionCodes.BillingViewReports);
    Summary(s => s.Summary = "Get billing settings");
  }
}
