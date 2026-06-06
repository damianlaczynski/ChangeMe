using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class UpdateLeaveType(IMediator mediator)
  : BaseEndpoint<UpdateLeaveTypeCommand, LeaveTypeDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/leave-types/{Id}");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Update leave type");
  }
}
