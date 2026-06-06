using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetLeaveBalance(IMediator mediator)
  : BaseEndpoint<GetLeaveBalanceQuery, LeaveBalanceDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/leave-balance");
    Summary(s => s.Summary = "Get leave balance for a user and year");
  }
}
