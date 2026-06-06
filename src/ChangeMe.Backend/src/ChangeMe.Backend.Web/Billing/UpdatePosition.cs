using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class UpdatePosition(IMediator mediator)
  : BaseEndpoint<UpdatePositionCommand, PositionDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/positions/{id}");
    RequirePermission(PermissionCodes.BillingManageEmployment);
    Summary(s => s.Summary = "Update position");
  }
}
