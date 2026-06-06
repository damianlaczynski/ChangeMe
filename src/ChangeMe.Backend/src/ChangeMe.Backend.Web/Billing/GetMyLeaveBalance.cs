using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetMyLeaveBalance(IMediator mediator)
  : BaseEndpointWithoutRequest<GetMyLeaveBalanceQuery, LeaveBalanceDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/leave-balance");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get signed-in user leave balance");
  }
}
