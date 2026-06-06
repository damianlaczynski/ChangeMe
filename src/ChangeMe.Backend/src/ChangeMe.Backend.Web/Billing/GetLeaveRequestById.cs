using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetLeaveRequestById(IMediator mediator)
  : BaseEndpoint<GetLeaveRequestByIdQuery, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/leave-requests/{Id}");
    Summary(s => s.Summary = "Get leave request details");
  }
}
