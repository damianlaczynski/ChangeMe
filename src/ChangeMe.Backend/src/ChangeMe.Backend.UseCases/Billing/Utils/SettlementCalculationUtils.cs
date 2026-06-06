using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal sealed class UserSettlementCalculationResult
{
  public Guid? ContractId { get; init; }
  public string PositionName { get; init; } = "—";
  public ContractType? ContractType { get; init; }
  public DateOnly? ContractStartDate { get; init; }
  public DateOnly? ContractEndDate { get; init; }
  public decimal? ContractFte { get; init; }
  public int MonthlyHoursNormMinutes { get; init; }
  public int ActiveContractDays { get; init; }
  public int DaysInMonth { get; init; }
  public int ExpectedMinutes { get; init; }
  public int PaidLeaveMinutesDeducted { get; init; }
  public int LoggedMinutes { get; init; }
  public decimal LeaveDays { get; init; }
  public int BalanceMinutes { get; init; }
  public IReadOnlyList<UserSettlementLeaveCalculationItem> LeaveItems { get; init; } = [];
}

internal sealed class UserSettlementLeaveCalculationItem
{
  public required string LeaveTypeName { get; init; }
  public required DateOnly StartDate { get; init; }
  public required DateOnly EndDate { get; init; }
  public LeaveDayPortion? DayPortion { get; init; }
  public decimal Days { get; init; }
  public bool CountsAsPaid { get; init; }
}

internal static class SettlementCalculationUtils
{
  private const int StandardWorkDaysPerMonth = 22;
  private const int DefaultFullDayMinutes = 480;
  private const int DefaultHalfDayMinutes = 240;

  public static async Task<List<Guid>> GetEligibleUserIdsAsync(
    ApplicationDbContext context,
    DateOnly monthStart,
    DateOnly monthEnd,
    CancellationToken cancellationToken)
  {
    var contractUserIds = await context.EmploymentContracts
      .AsNoTracking()
      .Where(c => c.StartDate <= monthEnd && (c.EndDate == null || c.EndDate >= monthStart))
      .Select(c => c.UserId)
      .Distinct()
      .ToListAsync(cancellationToken);

    var timeUserIds = await context.TimeEntries
      .AsNoTracking()
      .Where(t => t.WorkDate >= monthStart && t.WorkDate <= monthEnd)
      .Select(t => t.AuthorUserId)
      .Distinct()
      .ToListAsync(cancellationToken);

    return contractUserIds.Union(timeUserIds).Distinct().ToList();
  }

  public static async Task<UserSettlementCalculationResult> CalculateForUserAsync(
    ApplicationDbContext context,
    Guid userId,
    int year,
    int month,
    CancellationToken cancellationToken)
  {
    var monthStart = new DateOnly(year, month, 1);
    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
    var daysInMonth = monthEnd.DayNumber - monthStart.DayNumber + 1;

    var contracts = await (
      from contract in context.EmploymentContracts.AsNoTracking()
      join position in context.Positions.AsNoTracking() on contract.PositionId equals position.Id
      where contract.UserId == userId
            && contract.StartDate <= monthEnd
            && (contract.EndDate == null || contract.EndDate >= monthStart)
      select new ContractSnapshot
      {
        Id = contract.Id,
        PositionName = position.Name,
        ContractType = contract.ContractType,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        Fte = contract.Fte,
        MonthlyHoursNormMinutes = contract.MonthlyHoursNormMinutes,
      }).ToListAsync(cancellationToken);

    var majorityContract = DetermineMajorityContract(contracts, monthStart, monthEnd);

    var dailyNormMinutes = DefaultFullDayMinutes;
    var baseExpectedMinutes = 0;
    var activeContractDays = 0;

    if (majorityContract is not null)
    {
      activeContractDays = CountActiveDays(
        majorityContract.StartDate,
        majorityContract.EndDate,
        monthStart,
        monthEnd);
      dailyNormMinutes = CalculateDailyNormMinutes(majorityContract.MonthlyHoursNormMinutes);
      baseExpectedMinutes = (int)Math.Round(
        majorityContract.MonthlyHoursNormMinutes * (activeContractDays / (decimal)daysInMonth),
        MidpointRounding.AwayFromZero);
    }

    var approvedLeave = await (
      from request in context.LeaveRequests.AsNoTracking()
      join leaveType in context.LeaveTypes.AsNoTracking() on request.LeaveTypeId equals leaveType.Id
      where request.UserId == userId
            && request.Status == LeaveRequestStatus.Approved
            && request.StartDate <= monthEnd
            && request.EndDate >= monthStart
      select new LeaveSnapshot
      {
        LeaveTypeName = leaveType.Name,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        DayPortion = request.DayPortion,
        CountsAsPaid = leaveType.CountsAsPaid,
      }).ToListAsync(cancellationToken);

    decimal leaveDays = 0m;
    var paidLeaveMinutesDeducted = 0;
    var leaveItems = new List<UserSettlementLeaveCalculationItem>();

    foreach (var leave in approvedLeave)
    {
      var daysInPeriod = CountLeaveDaysInPeriod(
        leave.StartDate,
        leave.EndDate,
        leave.DayPortion,
        monthStart,
        monthEnd);
      if (daysInPeriod <= 0m)
        continue;

      leaveDays += daysInPeriod;
      leaveItems.Add(new UserSettlementLeaveCalculationItem
      {
        LeaveTypeName = leave.LeaveTypeName,
        StartDate = leave.StartDate,
        EndDate = leave.EndDate,
        DayPortion = leave.DayPortion,
        Days = daysInPeriod,
        CountsAsPaid = leave.CountsAsPaid,
      });

      if (leave.CountsAsPaid)
        paidLeaveMinutesDeducted += CalculatePaidLeaveMinutes(
          leave.StartDate,
          leave.EndDate,
          leave.DayPortion,
          monthStart,
          monthEnd,
          dailyNormMinutes);
    }

    leaveDays = decimal.Round(leaveDays, 1);

    var expectedMinutes = Math.Max(0, baseExpectedMinutes - paidLeaveMinutesDeducted);

    var loggedMinutes = await context.TimeEntries
      .AsNoTracking()
      .Where(t => t.AuthorUserId == userId && t.WorkDate >= monthStart && t.WorkDate <= monthEnd)
      .SumAsync(t => (int?)t.DurationMinutes, cancellationToken) ?? 0;

    return new UserSettlementCalculationResult
    {
      ContractId = majorityContract?.Id,
      PositionName = majorityContract?.PositionName ?? "—",
      ContractType = majorityContract?.ContractType,
      ContractStartDate = majorityContract?.StartDate,
      ContractEndDate = majorityContract?.EndDate,
      ContractFte = majorityContract?.Fte,
      MonthlyHoursNormMinutes = majorityContract?.MonthlyHoursNormMinutes ?? 0,
      ActiveContractDays = activeContractDays,
      DaysInMonth = daysInMonth,
      ExpectedMinutes = expectedMinutes,
      PaidLeaveMinutesDeducted = paidLeaveMinutesDeducted,
      LoggedMinutes = loggedMinutes,
      LeaveDays = leaveDays,
      BalanceMinutes = loggedMinutes - expectedMinutes,
      LeaveItems = leaveItems,
    };
  }

  public static async Task UpsertUserSettlementAsync(
    ApplicationDbContext context,
    Guid settlementPeriodId,
    Guid userId,
    UserSettlementCalculationResult calculation,
    DateTime calculatedAt,
    CancellationToken cancellationToken)
  {
    var existing = await context.UserSettlements
      .FirstOrDefaultAsync(
        s => s.SettlementPeriodId == settlementPeriodId && s.UserId == userId,
        cancellationToken);

    if (existing is null)
    {
      await context.UserSettlements.AddAsync(
        UserSettlement.Create(
          settlementPeriodId,
          userId,
          calculation.ContractId,
          calculation.ExpectedMinutes,
          calculation.LoggedMinutes,
          calculation.LeaveDays,
          calculatedAt),
        cancellationToken);
      return;
    }

    existing.UpdateCalculation(
      calculation.ContractId,
      calculation.ExpectedMinutes,
      calculation.LoggedMinutes,
      calculation.LeaveDays,
      calculatedAt);
  }

  public static string? BuildProrationNote(UserSettlementCalculationResult calculation)
  {
    if (calculation.ContractId is null || calculation.ActiveContractDays >= calculation.DaysInMonth)
      return null;

    return $"Contract active for {calculation.ActiveContractDays} of {calculation.DaysInMonth} days in the period.";
  }

  private static ContractSnapshot? DetermineMajorityContract(
    IReadOnlyList<ContractSnapshot> contracts,
    DateOnly monthStart,
    DateOnly monthEnd)
  {
    ContractSnapshot? best = null;
    var bestDays = 0;

    foreach (var contract in contracts)
    {
      var activeDays = CountActiveDays(contract.StartDate, contract.EndDate, monthStart, monthEnd);
      if (activeDays > bestDays
          || (activeDays == bestDays && best is not null && contract.StartDate > best.StartDate)
          || (activeDays == bestDays && best is null))
      {
        best = contract;
        bestDays = activeDays;
      }
    }

    return bestDays > 0 ? best : null;
  }

  private static int CountActiveDays(
    DateOnly contractStart,
    DateOnly? contractEnd,
    DateOnly monthStart,
    DateOnly monthEnd)
  {
    var effectiveStart = contractStart > monthStart ? contractStart : monthStart;
    var effectiveEnd = contractEnd.HasValue && contractEnd.Value < monthEnd
      ? contractEnd.Value
      : monthEnd;

    if (effectiveEnd < effectiveStart)
      return 0;

    return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
  }

  private static int CalculateDailyNormMinutes(int monthlyHoursNormMinutes) =>
    monthlyHoursNormMinutes > 0
      ? (int)Math.Round(monthlyHoursNormMinutes / (decimal)StandardWorkDaysPerMonth, MidpointRounding.AwayFromZero)
      : DefaultFullDayMinutes;

  private static decimal CountLeaveDaysInPeriod(
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    DateOnly periodStart,
    DateOnly periodEnd)
  {
    var effectiveStart = startDate < periodStart ? periodStart : startDate;
    var effectiveEnd = endDate > periodEnd ? periodEnd : endDate;
    if (effectiveEnd < effectiveStart)
      return 0m;

    if (effectiveStart == effectiveEnd && startDate == endDate)
    {
      return dayPortion is LeaveDayPortion.FirstHalf or LeaveDayPortion.SecondHalf ? 0.5m : 1m;
    }

    return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
  }

  private static int CalculatePaidLeaveMinutes(
    DateOnly startDate,
    DateOnly endDate,
    LeaveDayPortion? dayPortion,
    DateOnly periodStart,
    DateOnly periodEnd,
    int dailyNormMinutes)
  {
    var effectiveStart = startDate < periodStart ? periodStart : startDate;
    var effectiveEnd = endDate > periodEnd ? periodEnd : endDate;
    if (effectiveEnd < effectiveStart)
      return 0;

    var halfDayMinutes = dailyNormMinutes / 2;
    var total = 0;

    if (effectiveStart == effectiveEnd && startDate == endDate)
    {
      return dayPortion is LeaveDayPortion.FirstHalf or LeaveDayPortion.SecondHalf
        ? halfDayMinutes
        : dailyNormMinutes;
    }

    for (var day = effectiveStart; day <= effectiveEnd; day = day.AddDays(1))
    {
      if (day == startDate && day == endDate)
      {
        total += dayPortion is LeaveDayPortion.FirstHalf or LeaveDayPortion.SecondHalf
          ? halfDayMinutes
          : dailyNormMinutes;
        continue;
      }

      if (day == startDate && dayPortion is LeaveDayPortion.SecondHalf)
      {
        total += halfDayMinutes;
        continue;
      }

      if (day == endDate && dayPortion is LeaveDayPortion.FirstHalf)
      {
        total += halfDayMinutes;
        continue;
      }

      total += dailyNormMinutes;
    }

    return total;
  }

  private sealed class ContractSnapshot
  {
    public Guid Id { get; init; }
    public string PositionName { get; init; } = string.Empty;
    public ContractType ContractType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal Fte { get; init; }
    public int MonthlyHoursNormMinutes { get; init; }
  }

  private sealed class LeaveSnapshot
  {
    public string LeaveTypeName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public LeaveDayPortion? DayPortion { get; init; }
    public bool CountsAsPaid { get; init; }
  }
}
