using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class LeaveRequestsUtils
{
  public static bool Overlaps(
    DateOnly startDate,
    DateOnly endDate,
    DateOnly otherStartDate,
    DateOnly otherEndDate) =>
    startDate <= otherEndDate && otherStartDate <= endDate;

  public static async Task<Result> EnsureNoLeaveOverlapAsync(
    ApplicationDbContext context,
    Guid userId,
    DateOnly startDate,
    DateOnly endDate,
    Guid? excludeRequestId,
    CancellationToken cancellationToken)
  {
    var existing = await context.LeaveRequests
      .AsNoTracking()
      .Where(r => r.UserId == userId
                  && (excludeRequestId == null || r.Id != excludeRequestId)
                  && (r.Status == LeaveRequestStatus.Approved || r.Status == LeaveRequestStatus.Submitted))
      .Select(r => new { r.StartDate, r.EndDate })
      .ToListAsync(cancellationToken);

    foreach (var request in existing)
    {
      if (Overlaps(startDate, endDate, request.StartDate, request.EndDate))
        return Result.Conflict(BillingConstraints.LeaveOverlapMessage);
    }

    return Result.Success();
  }

  public static Result ValidateStartDateWindow(DateOnly startDate, DateOnly today)
  {
    var earliest = new DateOnly(today.Year, today.Month, 1).AddMonths(-BillingConstraints.LeaveBackdatingMonths);
    return startDate >= earliest
      ? Result.Success()
      : Result.Invalid(new ValidationError(
        nameof(LeaveRequest.StartDate),
        $"Start date must not be before {earliest:yyyy-MM-dd}."));
  }

  public static decimal CalculateDays(DateOnly startDate, DateOnly endDate, LeaveDayPortion? dayPortion)
  {
    if (startDate == endDate)
    {
      return dayPortion is LeaveDayPortion.FirstHalf or LeaveDayPortion.SecondHalf ? 0.5m : 1m;
    }

    return endDate.DayNumber - startDate.DayNumber + 1;
  }

  public static LeaveDayPortion? ResolveDayPortion(
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    bool allowHalfDayLeave)
  {
    if (startDate != endDate)
      return null;

    if (!allowHalfDayLeave)
      return LeaveDayPortion.FullDay;

    return dayPortion ?? LeaveDayPortion.FullDay;
  }

  public static string FormatDates(DateOnly startDate, DateOnly endDate) =>
    startDate == endDate
      ? startDate.ToString("dd.MM.yyyy")
      : $"{startDate:dd.MM.yyyy} – {endDate:dd.MM.yyyy}";

  public static bool CanViewLeaveRequests(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.BillingViewAny)
    || userAccessor.HasPermission(PermissionCodes.BillingManageLeave)
    || userAccessor.HasPermission(PermissionCodes.BillingApproveLeave);

  public static bool CanFilterByUsers(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.BillingViewAny)
    || userAccessor.HasPermission(PermissionCodes.BillingManageLeave);

  public static LeaveRequestDetailsDto MapDetails(
    LeaveRequest request,
    LeaveRequestDetailsContext context)
  {
    return new LeaveRequestDetailsDto
    {
      Id = request.Id,
      UserId = request.UserId,
      UserDisplayName = context.UserDisplayName,
      LeaveTypeId = request.LeaveTypeId,
      LeaveTypeName = context.LeaveTypeName,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      DayPortion = request.DayPortion,
      Days = CalculateDays(request.StartDate, request.EndDate, request.DayPortion),
      Status = request.Status,
      Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason,
      SubmittedAt = request.SubmittedAt,
      DecidedAt = request.DecidedAt,
      DecidedByUserId = request.DecidedByUserId,
      DecidedByDisplayName = context.DecidedByDisplayName,
      RejectReason = string.IsNullOrWhiteSpace(request.RejectReason) ? null : request.RejectReason,
      CanEdit = context.CanEdit,
      CanSubmit = context.CanSubmit,
      CanApprove = context.CanApprove,
      CanReject = context.CanReject,
      CanCancel = context.CanCancel,
      CanDelete = context.CanDelete,
    };
  }

  public static LeaveRequestListItemDto MapListItem(
    LeaveRequest request,
    string userDisplayName,
    string leaveTypeName)
  {
    return new LeaveRequestListItemDto
    {
      Id = request.Id,
      UserId = request.UserId,
      UserDisplayName = userDisplayName,
      LeaveTypeName = leaveTypeName,
      DatesDisplay = FormatDates(request.StartDate, request.EndDate),
      Days = CalculateDays(request.StartDate, request.EndDate, request.DayPortion),
      Status = request.Status,
      SubmittedAt = request.SubmittedAt,
      StartDate = request.StartDate,
    };
  }
}

internal sealed class LeaveRequestDetailsContext
{
  public required string UserDisplayName { get; init; }
  public required string LeaveTypeName { get; init; }
  public string? DecidedByDisplayName { get; init; }
  public bool CanEdit { get; init; }
  public bool CanSubmit { get; init; }
  public bool CanApprove { get; init; }
  public bool CanReject { get; init; }
  public bool CanCancel { get; init; }
  public bool CanDelete { get; init; }
}
