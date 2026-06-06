using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetLeaveTypeById(IMediator mediator)
  : BaseEndpoint<GetLeaveTypeByIdQuery, LeaveTypeDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/leave-types/{Id}");
    RequirePermission(PermissionCodes.BillingViewReports);
    Summary(s => s.Summary = "Get leave type details");
  }
}
