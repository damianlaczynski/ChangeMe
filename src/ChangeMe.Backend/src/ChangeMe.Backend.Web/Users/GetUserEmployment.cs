using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.Web.Authorization;

namespace ChangeMe.Backend.Web.Users;

public class GetUserEmployment(IMediator mediator)
  : BaseEndpoint<GetUserEmploymentQuery, UserEmploymentDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/users/{Id}/employment");
    RequirePermission(BillingAuthorizationPolicies.ViewEmployment);
    Summary(s => s.Summary = "Get user employment profile and contracts");
  }
}
