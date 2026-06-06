using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class CreateMyLeaveRequest(IMediator mediator)
  : BaseEndpoint<CreateMyLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/my/leave-requests");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Create own leave request");
  }
}
