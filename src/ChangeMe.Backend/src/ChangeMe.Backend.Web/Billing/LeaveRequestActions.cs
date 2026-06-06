using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class UpdateLeaveRequest(IMediator mediator)
  : BaseEndpoint<UpdateLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/leave-requests/{Id}");
    Summary(s => s.Summary = "Update leave request");
  }
}

public class SubmitLeaveRequest(IMediator mediator)
  : BaseEndpoint<SubmitLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/leave-requests/{Id}/submit");
    Summary(s => s.Summary = "Submit leave request");
  }
}

public class ApproveLeaveRequest(IMediator mediator)
  : BaseEndpoint<ApproveLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/leave-requests/{Id}/approve");
    Summary(s => s.Summary = "Approve leave request");
  }
}

public class RejectLeaveRequest(IMediator mediator)
  : BaseEndpoint<RejectLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/leave-requests/{Id}/reject");
    Summary(s => s.Summary = "Reject leave request");
  }
}

public class CancelLeaveRequest(IMediator mediator)
  : BaseEndpoint<CancelLeaveRequestCommand, LeaveRequestDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/leave-requests/{Id}/cancel");
    Summary(s => s.Summary = "Cancel leave request");
  }
}

public class DeleteLeaveRequest(IMediator mediator)
  : BaseEndpoint<DeleteLeaveRequestCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/billing/leave-requests/{Id}");
    Summary(s => s.Summary = "Delete draft leave request");
  }
}
