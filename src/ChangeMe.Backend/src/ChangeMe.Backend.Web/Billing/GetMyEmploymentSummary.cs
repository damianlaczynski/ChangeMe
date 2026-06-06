using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetMyEmploymentSummary(IMediator mediator)
  : BaseEndpointWithoutRequest<GetMyEmploymentSummaryQuery, MyEmploymentSummaryDto?>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/employment");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get signed-in user active employment summary");
  }
}
