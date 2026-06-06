using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;

namespace ChangeMe.Backend.Web.Billing;

public class DeleteLeaveType(IMediator mediator)
  : BaseEndpoint<DeleteLeaveTypeCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/billing/leave-types/{Id}");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Delete leave type");
  }
}
