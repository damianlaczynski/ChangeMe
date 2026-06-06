using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.Web.Authorization;

namespace ChangeMe.Backend.Web.Users;

public class GetEmploymentContractById(IMediator mediator)
  : BaseEndpoint<GetEmploymentContractByIdQuery, EmploymentContractDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/users/{Id}/employment/contracts/{ContractId}");
    RequirePermission(BillingAuthorizationPolicies.ViewEmployment);
    Summary(s => s.Summary = "Get employment contract details");
  }
}
