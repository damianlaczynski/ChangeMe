using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;

namespace ChangeMe.Backend.Web.Billing;

public class DeletePosition(IMediator mediator) : BaseEndpoint<DeletePositionCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/billing/positions/{id}");
    RequirePermission(PermissionCodes.BillingManageEmployment);
    Summary(s => s.Summary = "Delete position");
  }
}
