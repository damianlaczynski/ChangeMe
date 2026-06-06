using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetTimeReports(IMediator mediator) : BaseEndpoint<GetTimeReportsQuery, TimeReportResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/reports");
    RequirePermission(PermissionCodes.TimeViewReports);
    Summary(s => s.Summary = "Run time report");
  }
}
