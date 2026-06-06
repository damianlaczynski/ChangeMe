using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetTimeEntryAuditLog(IMediator mediator)
  : BaseEndpoint<GetTimeEntryAuditLogQuery, PaginationResult<TimeEntryAuditLogEntryDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/audit-log");
    RequirePermission(PermissionCodes.TimeViewReports);
    Summary(s => s.Summary = "Get time entry audit log");
  }
}
