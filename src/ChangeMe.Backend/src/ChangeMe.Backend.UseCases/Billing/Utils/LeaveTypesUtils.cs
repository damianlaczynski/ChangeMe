using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class LeaveTypesUtils
{
  public static async Task<Result> EnsureUniqueLeaveTypeNameAsync(
    ApplicationDbContext context,
    string name,
    Guid? excludeLeaveTypeId,
    CancellationToken cancellationToken)
  {
    var normalizedName = LeaveType.NormalizeName(name);
    var exists = await context.LeaveTypes
      .AsNoTracking()
      .AnyAsync(
        lt => lt.NormalizedName == normalizedName
              && (!excludeLeaveTypeId.HasValue || lt.Id != excludeLeaveTypeId.Value),
        cancellationToken);

    return exists
      ? Result.Conflict(BillingConstraints.LeaveTypeNameDuplicateMessage)
      : Result.Success();
  }

  public static async Task<Result> EnsureUniqueLeaveTypeCodeAsync(
    ApplicationDbContext context,
    string code,
    Guid? excludeLeaveTypeId,
    CancellationToken cancellationToken)
  {
    var normalizedCode = LeaveType.NormalizeCode(code);
    var exists = await context.LeaveTypes
      .AsNoTracking()
      .AnyAsync(
        lt => lt.NormalizedCode == normalizedCode
              && (!excludeLeaveTypeId.HasValue || lt.Id != excludeLeaveTypeId.Value),
        cancellationToken);

    return exists
      ? Result.Conflict(BillingConstraints.LeaveTypeCodeDuplicateMessage)
      : Result.Success();
  }

  public static LeaveTypeListItemDto MapListItem(LeaveType leaveType, bool canManage)
  {
    return new LeaveTypeListItemDto
    {
      Id = leaveType.Id,
      Name = leaveType.Name,
      Code = leaveType.Code,
      CountsAsPaid = leaveType.CountsAsPaid,
      UsesAllowance = leaveType.UsesAllowance,
      RequiresApproval = leaveType.RequiresApproval,
      IsActive = leaveType.IsActive,
      IsSeeded = leaveType.IsSeeded,
      CanManage = canManage,
      CanDelete = canManage && !leaveType.IsSeeded,
    };
  }

  public static LeaveTypeDetailsDto MapDetails(LeaveType leaveType, bool canManage, bool canDelete)
  {
    return new LeaveTypeDetailsDto
    {
      Id = leaveType.Id,
      Name = leaveType.Name,
      Code = leaveType.Code,
      CountsAsPaid = leaveType.CountsAsPaid,
      UsesAllowance = leaveType.UsesAllowance,
      RequiresApproval = leaveType.RequiresApproval,
      IsActive = leaveType.IsActive,
      IsSeeded = leaveType.IsSeeded,
      CanManage = canManage,
      CanDelete = canDelete,
    };
  }

  public static async Task<bool> HasLeaveRequestsAsync(
    ApplicationDbContext context,
    Guid leaveTypeId,
    CancellationToken cancellationToken) =>
    await context.LeaveRequests
      .AsNoTracking()
      .AnyAsync(r => r.LeaveTypeId == leaveTypeId, cancellationToken);
}

internal static class BillingSettingsUtils
{
  public const string InvalidTimeMessage = "Enter a valid time.";

  public static Result<(TimeOnly Start, TimeOnly End, TimeOnly Split)> ParseWorkdayTimes(
    string defaultWorkdayStart,
    string defaultWorkdayEnd,
    string halfDaySplitTime)
  {
    if (!TimeOnly.TryParse(defaultWorkdayStart, out var start))
      return Result.Invalid(new ValidationError(nameof(BillingSettings.DefaultWorkdayStart), InvalidTimeMessage));

    if (!TimeOnly.TryParse(defaultWorkdayEnd, out var end))
      return Result.Invalid(new ValidationError(nameof(BillingSettings.DefaultWorkdayEnd), InvalidTimeMessage));

    if (!TimeOnly.TryParse(halfDaySplitTime, out var split))
      return Result.Invalid(new ValidationError(nameof(BillingSettings.HalfDaySplitTime), InvalidTimeMessage));

    return Result.Success((start, end, split));
  }

  public static BillingSettingsDto MapDto(BillingSettings settings, bool canEdit)
  {
    var workdays = settings.GetDefaultWorkdays();
    return new BillingSettingsDto
    {
      DefaultAnnualLeaveDays = settings.DefaultAnnualLeaveDays,
      AllowHalfDayLeave = settings.AllowHalfDayLeave,
      DefaultWorkdayStart = settings.DefaultWorkdayStart.ToString("HH:mm"),
      DefaultWorkdayEnd = settings.DefaultWorkdayEnd.ToString("HH:mm"),
      HalfDaySplitTime = settings.HalfDaySplitTime.ToString("HH:mm"),
      DefaultWorkdays = workdays,
      DefaultAvailabilityStatus = settings.DefaultAvailabilityStatus,
      DefaultWorkdayDurationMinutes = (int)(settings.DefaultWorkdayEnd - settings.DefaultWorkdayStart).TotalMinutes,
      CanEdit = canEdit,
    };
  }
}

internal static class LeaveBalanceUtils
{
  public static async Task<LeaveBalanceDto?> CalculateAsync(
    ApplicationDbContext context,
    Guid userId,
    int year,
    DateOnly calculationDate,
    CancellationToken cancellationToken)
  {
    var settings = await context.BillingSettings
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
    if (settings is null)
      return null;

    var activeContract = await context.EmploymentContracts
      .AsNoTracking()
      .Where(c => c.UserId == userId
                  && c.StartDate <= calculationDate
                  && (c.EndDate == null || c.EndDate >= calculationDate))
      .OrderByDescending(c => c.StartDate)
      .FirstOrDefaultAsync(cancellationToken);

    if (activeContract is null)
    {
      return new LeaveBalanceDto
      {
        UserId = userId,
        Year = year,
        EntitledDays = 0m,
        UsedDays = 0m,
        RemainingDays = 0m,
      };
    }

    var entitledDays = activeContract.ContractType == ContractType.Employment
      ? decimal.Round(settings.DefaultAnnualLeaveDays * activeContract.Fte, 1)
      : 0m;

    var allowanceTypeIds = await context.LeaveTypes
      .AsNoTracking()
      .Where(lt => lt.UsesAllowance)
      .Select(lt => lt.Id)
      .ToListAsync(cancellationToken);

    var yearStart = new DateOnly(year, 1, 1);
    var yearEnd = new DateOnly(year, 12, 31);

    var approvedRequests = await context.LeaveRequests
      .AsNoTracking()
      .Where(r => r.UserId == userId
                  && r.Status == LeaveRequestStatus.Approved
                  && allowanceTypeIds.Contains(r.LeaveTypeId)
                  && r.StartDate <= yearEnd
                  && r.EndDate >= yearStart)
      .Select(r => new { r.StartDate, r.EndDate, r.DayPortion })
      .ToListAsync(cancellationToken);

    decimal usedDays = 0m;
    foreach (var request in approvedRequests)
    {
      usedDays += CountLeaveDaysInYear(request.StartDate, request.EndDate, request.DayPortion, yearStart, yearEnd);
    }

    usedDays = decimal.Round(usedDays, 1);

    return new LeaveBalanceDto
    {
      UserId = userId,
      Year = year,
      EntitledDays = entitledDays,
      UsedDays = usedDays,
      RemainingDays = decimal.Round(entitledDays - usedDays, 1),
    };
  }

  private static decimal CountLeaveDaysInYear(
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    DateOnly yearStart,
    DateOnly yearEnd)
  {
    var effectiveStart = startDate < yearStart ? yearStart : startDate;
    var effectiveEnd = endDate > yearEnd ? yearEnd : endDate;
    if (effectiveEnd < effectiveStart)
      return 0m;

    if (effectiveStart == effectiveEnd)
    {
      return dayPortion is LeaveDayPortion.FirstHalf or LeaveDayPortion.SecondHalf ? 0.5m : 1m;
    }

    return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
  }
}
