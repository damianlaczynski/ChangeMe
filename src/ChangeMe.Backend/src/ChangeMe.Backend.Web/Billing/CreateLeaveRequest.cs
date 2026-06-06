using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class CreateLeaveRequest(IMediator mediator)
  : BaseEndpoint<CreateLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/leave-requests");
    RequirePermission(PermissionCodes.BillingManageLeave);
    Summary(s => s.Summary = "Create leave request for a user");
  }
}
