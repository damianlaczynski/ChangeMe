using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetReportPersonEntries(IMediator mediator)
  : BaseEndpoint<GetReportPersonEntriesQuery, PaginationResult<ReportPersonEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/reports/person-entries");
    RequirePermission(PermissionCodes.TimeViewReports);
    Summary(s => s.Summary = "Get paginated time entries for a user in report drill-down");
  }
}
