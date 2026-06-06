using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class CreateEmploymentContract(IMediator mediator)
  : BaseEndpoint<CreateEmploymentContractCommand, EmploymentContractDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/users/{Id}/employment/contracts");
    RequirePermission(PermissionCodes.BillingManageEmployment);
    Summary(s => s.Summary = "Create employment contract");
  }
}
