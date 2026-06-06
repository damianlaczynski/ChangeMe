using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class UpdateBillingSettings(IMediator mediator)
  : BaseEndpoint<UpdateBillingSettingsCommand, BillingSettingsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/settings");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Update billing settings");
  }
}
