using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class CreatePosition(IMediator mediator)
  : BaseEndpoint<CreatePositionCommand, PositionDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/positions");
    RequirePermission(PermissionCodes.BillingManageEmployment);
    Summary(s => s.Summary = "Create position");
  }
}
