using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class UpdateEmploymentContract(IMediator mediator)
  : BaseEndpoint<UpdateEmploymentContractCommand, EmploymentContractDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/users/{Id}/employment/contracts/{ContractId}");
    RequirePermission(PermissionCodes.BillingManageEmployment);
    Summary(s => s.Summary = "Update employment contract");
  }
}
