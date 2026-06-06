using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetMyLeaveRequests(IMediator mediator)
  : BaseEndpoint<GetMyLeaveRequestsQuery, PaginationResult<LeaveRequestListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/leave-requests");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get signed-in user leave requests");
  }
}
