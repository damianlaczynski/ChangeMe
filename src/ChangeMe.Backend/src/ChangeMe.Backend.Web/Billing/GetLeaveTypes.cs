using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetLeaveTypes(IMediator mediator)
  : BaseEndpointWithoutRequest<GetLeaveTypesQuery, IReadOnlyList<LeaveTypeListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/leave-types");
    RequirePermission(PermissionCodes.BillingViewReports);
    Summary(s => s.Summary = "Get leave types");
  }
}
