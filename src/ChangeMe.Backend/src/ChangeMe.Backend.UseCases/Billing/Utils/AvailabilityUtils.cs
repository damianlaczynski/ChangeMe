using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class AvailabilityUtils
{
  public const int RecurringHorizonDays = 365;
  public const int TeamCalendarUserCap = 50;

  private static readonly TimeOnly FallbackWorkdayStart = new(9, 0);
  private static readonly TimeOnly FallbackWorkdayEnd = new(17, 0);
  private static readonly TimeOnly FallbackHalfDaySplit = new(13, 0);

  public static bool CanManageUserAvailability(IUserAccessor userAccessor, Guid targetUserId)
  {
    if (userAccessor.UserId is null)
      return false;

    if (targetUserId == userAccessor.UserId.Value)
      return userAccessor.HasPermission(PermissionCodes.BillingManageOwnAvailability);

    return userAccessor.HasPermission(PermissionCodes.BillingManageAvailability);
  }

  public static bool CanViewUserAvailability(IUserAccessor userAccessor, Guid targetUserId)
  {
    if (userAccessor.UserId is null)
      return false;

    if (targetUserId == userAccessor.UserId.Value)
      return userAccessor.HasPermission(PermissionCodes.BillingViewOwn);

    return userAccessor.HasPermission(PermissionCodes.BillingViewAny);
  }

  public static bool DateRangesOverlap(
    DateOnly startDate,
    DateOnly endDate,
    DateOnly otherStartDate,
    DateOnly otherEndDate) =>
    startDate <= otherEndDate && otherStartDate <= endDate;

  public static bool TimesOverlap(
    TimeOnly startTime,
    TimeOnly endTime,
    TimeOnly otherStartTime,
    TimeOnly otherEndTime) =>
    startTime < otherEndTime && otherStartTime < endTime;

  public static bool ManualEntriesOverlapOnDay(
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityEntry other,
    DateOnly day)
  {
    if (other.Source != AvailabilityEntrySource.Manual)
      return false;

    if (day < startDate || day > endDate || day < other.StartDate || day > other.EndDate)
      return false;

    if (allDay || other.AllDay)
      return true;

    return TimesOverlap(
      startTime!.Value,
      endTime!.Value,
      other.StartTime!.Value,
      other.EndTime!.Value);
  }

  public static async Task<Result> EnsureNoManualOverlapAsync(
    ApplicationDbContext context,
    Guid userId,
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    Guid? excludeEntryId,
    CancellationToken cancellationToken)
  {
    var existing = await context.AvailabilityEntries
      .AsNoTracking()
      .Where(e => e.UserId == userId
                  && e.Source == AvailabilityEntrySource.Manual
                  && (excludeEntryId == null || e.Id != excludeEntryId)
                  && e.StartDate <= endDate
                  && e.EndDate >= startDate)
      .ToListAsync(cancellationToken);

    foreach (var entry in existing)
    {
      for (var day = startDate; day <= endDate; day = day.AddDays(1))
      {
        if (ManualEntriesOverlapOnDay(startDate, endDate, allDay, startTime, endTime, entry, day))
          return Result.Conflict(BillingConstraints.AvailabilityOverlapMessage);
      }
    }

    return Result.Success();
  }

  public static async Task RegenerateRecurringEntriesAsync(
    ApplicationDbContext context,
    WeeklyRecurringPattern pattern,
    DateOnly fromDate,
    CancellationToken cancellationToken)
  {
    var toDate = fromDate.AddDays(RecurringHorizonDays);

    var existing = await context.AvailabilityEntries
      .Where(e => e.UserId == pattern.UserId
                  && e.Source == AvailabilityEntrySource.Recurring
                  && e.StartDate >= fromDate
                  && e.StartDate <= toDate)
      .ToListAsync(cancellationToken);

    context.AvailabilityEntries.RemoveRange(existing);

    foreach (var day in pattern.Days.Where(d => d.Enabled && d.StartTime.HasValue && d.EndTime.HasValue && d.Status.HasValue))
    {
      for (var date = fromDate; date <= toDate; date = date.AddDays(1))
      {
        if (date.DayOfWeek != day.DayOfWeek)
          continue;

        var entryResult = AvailabilityEntry.CreateRecurring(
          pattern.UserId,
          date,
          day.StartTime!.Value,
          day.EndTime!.Value,
          day.Status!.Value);
        if (entryResult.IsSuccess)
          await context.AvailabilityEntries.AddAsync(entryResult.Value, cancellationToken);
      }
    }
  }

  public static async Task SyncLeaveEntriesAsync(
    ApplicationDbContext context,
    LeaveRequest request,
    string leaveTypeName,
    BillingSettings? settings,
    CancellationToken cancellationToken)
  {
    await RemoveLeaveEntriesAsync(context, request.Id, cancellationToken);

    var workdayStart = settings?.DefaultWorkdayStart ?? FallbackWorkdayStart;
    var workdayEnd = settings?.DefaultWorkdayEnd ?? FallbackWorkdayEnd;
    var halfDaySplit = settings?.HalfDaySplitTime ?? FallbackHalfDaySplit;

    for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
    {
      var allDay = true;
      TimeOnly? startTime = null;
      TimeOnly? endTime = null;

      if (request.StartDate == request.EndDate && request.DayPortion is not null and not LeaveDayPortion.FullDay)
      {
        allDay = false;
        if (request.DayPortion == LeaveDayPortion.FirstHalf)
        {
          startTime = workdayStart;
          endTime = halfDaySplit;
        }
        else
        {
          startTime = halfDaySplit;
          endTime = workdayEnd;
        }
      }

      var entryResult = AvailabilityEntry.CreateLeave(
        request.UserId,
        date,
        allDay,
        startTime,
        endTime,
        leaveTypeName,
        request.Id);
      if (entryResult.IsSuccess)
        await context.AvailabilityEntries.AddAsync(entryResult.Value, cancellationToken);
    }
  }

  public static async Task RemoveLeaveEntriesAsync(
    ApplicationDbContext context,
    Guid leaveRequestId,
    CancellationToken cancellationToken)
  {
    var entries = await context.AvailabilityEntries
      .Where(e => e.LeaveRequestId == leaveRequestId && e.Source == AvailabilityEntrySource.Leave)
      .ToListAsync(cancellationToken);

    context.AvailabilityEntries.RemoveRange(entries);
  }

  public static async Task<decimal?> GetActiveContractFteAsync(
    ApplicationDbContext context,
    Guid userId,
    DateOnly today,
    CancellationToken cancellationToken)
  {
    return await context.EmploymentContracts
      .AsNoTracking()
      .Where(c => c.UserId == userId
                  && c.StartDate <= today
                  && (c.EndDate == null || c.EndDate >= today))
      .OrderByDescending(c => c.StartDate)
      .Select(c => (decimal?)c.Fte)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public static AvailabilityEntryDto MapEntry(AvailabilityEntry entry) =>
    new()
    {
      Id = entry.Id,
      UserId = entry.UserId,
      StartDate = entry.StartDate,
      EndDate = entry.EndDate,
      AllDay = entry.AllDay,
      StartTime = entry.StartTime,
      EndTime = entry.EndTime,
      Status = entry.Status,
      Notes = entry.Notes,
      Source = entry.Source,
      LeaveRequestId = entry.LeaveRequestId,
      CreatedAt = entry.CreatedAt,
      UpdatedAt = entry.UpdatedAt,
    };

  public static WeeklyRecurringPatternDto MapPattern(WeeklyRecurringPattern? pattern) =>
    new()
    {
      UserId = pattern?.UserId ?? Guid.Empty,
      Days = Enum.GetValues<DayOfWeek>()
        .Select(day =>
        {
          var row = pattern?.Days.FirstOrDefault(d => d.DayOfWeek == day);
          return new WeeklyRecurringPatternDayDto
          {
            DayOfWeek = day,
            Enabled = row?.Enabled ?? false,
            StartTime = row?.StartTime,
            EndTime = row?.EndTime,
            Status = row?.Status,
          };
        })
        .ToList(),
    };

  public static int GetSourcePriority(AvailabilityEntrySource source) =>
    source switch
    {
      AvailabilityEntrySource.Leave => 0,
      AvailabilityEntrySource.Manual => 1,
      AvailabilityEntrySource.Recurring => 2,
      _ => 3,
    };

  public static AvailabilityEntryDto? ResolveEffectiveEntry(
    IReadOnlyList<AvailabilityEntryDto> entries,
    DateOnly day)
  {
    var dayEntries = entries
      .Where(e => day >= e.StartDate && day <= e.EndDate)
      .OrderBy(e => GetSourcePriority(e.Source))
      .ThenByDescending(e => e.AllDay)
      .ToList();

    return dayEntries.FirstOrDefault();
  }

  public static IReadOnlyList<AvailabilityEntryDto> GetEntriesForDay(
    IReadOnlyList<AvailabilityEntryDto> entries,
    DateOnly day) =>
    entries
      .Where(e => day >= e.StartDate && day <= e.EndDate)
      .OrderBy(e => GetSourcePriority(e.Source))
      .ThenBy(e => e.AllDay ? 0 : 1)
      .ThenBy(e => e.StartTime)
      .ToList();

}
