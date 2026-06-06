using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class CreateLeaveType(IMediator mediator)
  : BaseEndpoint<CreateLeaveTypeCommand, LeaveTypeDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/leave-types");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Create leave type");
  }
}
