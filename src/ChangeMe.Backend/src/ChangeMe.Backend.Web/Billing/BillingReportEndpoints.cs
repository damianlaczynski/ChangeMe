using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetBillingSettlementReport(IMediator mediator)
  : BaseEndpoint<GetBillingSettlementReportQuery, BillingSettlementReportResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/reports/settlements");
    RequirePermission(PermissionCodes.BillingViewReports);
    Summary(s => s.Summary = "Run billing settlement report");
  }
}

public class GetBillingLeaveReport(IMediator mediator)
  : BaseEndpoint<GetBillingLeaveReportQuery, BillingLeaveReportResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/reports/leave");
    RequirePermission(PermissionCodes.BillingViewReports);
    Summary(s => s.Summary = "Run billing leave report");
  }
}
