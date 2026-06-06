using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.UseCases.Billing.Dtos;

public class LeaveRequestListItemDto
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string UserDisplayName { get; set; } = string.Empty;
  public string LeaveTypeName { get; set; } = string.Empty;
  public string DatesDisplay { get; set; } = string.Empty;
  public decimal Days { get; set; }
  public LeaveRequestStatus Status { get; set; }
  public DateTime? SubmittedAt { get; set; }
  public DateOnly StartDate { get; set; }
}

public class LeaveRequestDetailsDto
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string UserDisplayName { get; set; } = string.Empty;
  public Guid LeaveTypeId { get; set; }
  public string LeaveTypeName { get; set; } = string.Empty;
  public DateOnly StartDate { get; set; }
  public DateOnly EndDate { get; set; }
  public LeaveDayPortion? DayPortion { get; set; }
  public decimal Days { get; set; }
  public LeaveRequestStatus Status { get; set; }
  public string? Reason { get; set; }
  public DateTime? SubmittedAt { get; set; }
  public DateTime? DecidedAt { get; set; }
  public Guid? DecidedByUserId { get; set; }
  public string? DecidedByDisplayName { get; set; }
  public string? RejectReason { get; set; }
  public bool CanEdit { get; set; }
  public bool CanSubmit { get; set; }
  public bool CanApprove { get; set; }
  public bool CanReject { get; set; }
  public bool CanCancel { get; set; }
  public bool CanDelete { get; set; }
}

public record SaveLeaveRequestRequest(
  Guid? UserId,
  Guid LeaveTypeId,
  DateOnly StartDate,
  DateOnly EndDate,
  LeaveDayPortion? DayPortion,
  string? Reason);

public record RejectLeaveRequestRequest(string RejectReason);

public class GetLeaveRequestsQuery : PaginationQuery<LeaveRequestListItemDto>
{
  public IReadOnlyList<LeaveRequestStatus>? Statuses { get; set; }
  public IReadOnlyList<Guid>? LeaveTypeIds { get; set; }
  public IReadOnlyList<Guid>? UserIds { get; set; }
  public DateOnly? DateFrom { get; set; }
  public DateOnly? DateTo { get; set; }
}

public class GetMyLeaveRequestsQuery : PaginationQuery<LeaveRequestListItemDto>
{
  public bool ShowAllYears { get; set; }
}
