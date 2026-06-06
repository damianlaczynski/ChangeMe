using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetLeaveRequests(IMediator mediator)
  : BaseEndpoint<GetLeaveRequestsQuery, PaginationResult<LeaveRequestListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/leave-requests");
    RequirePermissions(
      PermissionCodes.BillingViewAny,
      PermissionCodes.BillingManageLeave,
      PermissionCodes.BillingApproveLeave);
    Summary(s => s.Summary = "Get leave requests");
  }
}
