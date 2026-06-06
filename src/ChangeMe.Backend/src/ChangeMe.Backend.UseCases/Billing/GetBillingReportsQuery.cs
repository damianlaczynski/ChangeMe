using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public class GetBillingSettlementReportHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetBillingSettlementReportQuery, BillingSettlementReportResultDto>
{
  public async Task<Result<BillingSettlementReportResultDto>> Handle(
    GetBillingSettlementReportQuery query,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanViewBillingReports(userAccessor))
      return Result.Forbidden();

    var period = await context.SettlementPeriods
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == query.SettlementPeriodId, cancellationToken);
    if (period is null)
      return Result.NotFound();

    var rows = await (
      from settlement in context.UserSettlements.AsNoTracking()
      join user in context.Users.AsNoTracking() on settlement.UserId equals user.Id
      join contract in context.EmploymentContracts.AsNoTracking()
        on settlement.ContractId equals contract.Id into contracts
      from contract in contracts.DefaultIfEmpty()
      join position in context.Positions.AsNoTracking()
        on contract.PositionId equals position.Id into positions
      from position in positions.DefaultIfEmpty()
      where settlement.SettlementPeriodId == period.Id
      select new SettlementReportSourceRow
      {
        UserId = settlement.UserId,
        UserFirstName = user.FirstName,
        UserLastName = user.LastName,
        UserEmail = user.Email,
        PositionName = position != null ? position.Name : "—",
        ContractType = contract != null ? (ContractType?)contract.ContractType : null,
        ExpectedMinutes = settlement.ExpectedMinutes,
        LoggedMinutes = settlement.LoggedMinutes,
        LeaveDays = settlement.LeaveDays,
        BalanceMinutes = settlement.BalanceMinutes,
      }).ToListAsync(cancellationToken);

    if (query.UserIds is { Count: > 0 })
      rows = rows.Where(r => query.UserIds.Contains(r.UserId)).ToList();

    if (query.ContractTypes is { Count: > 0 })
    {
      rows = rows.Where(r =>
        r.ContractType.HasValue && query.ContractTypes.Contains(r.ContractType.Value)).ToList();
    }

    var reportRows = query.GroupingMode switch
    {
      BillingSettlementReportGroupingMode.ByPerson => BuildByPerson(rows),
      BillingSettlementReportGroupingMode.ByPosition => BuildByPosition(rows),
      BillingSettlementReportGroupingMode.ByContractType => BuildByContractType(rows),
      BillingSettlementReportGroupingMode.OvertimeSummary => BuildOvertime(rows),
      BillingSettlementReportGroupingMode.UndertimeSummary => BuildUndertime(rows),
      _ => BuildByPerson(rows),
    };

    return Result.Success(new BillingSettlementReportResultDto
    {
      GroupingMode = query.GroupingMode,
      PeriodYear = period.Year,
      PeriodMonth = period.Month,
      PeriodLabel = SettlementsUtils.FormatPeriodLabel(period.Year, period.Month),
      UserCount = rows.Select(r => r.UserId).Distinct().Count(),
      TotalExpectedMinutes = rows.Sum(r => r.ExpectedMinutes),
      TotalLoggedMinutes = rows.Sum(r => r.LoggedMinutes),
      NetBalanceMinutes = rows.Sum(r => r.BalanceMinutes),
      Rows = reportRows,
    });
  }

  private static List<BillingSettlementReportRowDto> BuildByPerson(List<SettlementReportSourceRow> rows) =>
    rows
      .OrderBy(r => r.DisplayName)
      .Select(r => new BillingSettlementReportRowDto
      {
        Label = r.DisplayName,
        UserId = r.UserId,
        UserDisplayName = r.DisplayName,
        ExpectedMinutes = r.ExpectedMinutes,
        LoggedMinutes = r.LoggedMinutes,
        LeaveDays = r.LeaveDays,
        BalanceMinutes = r.BalanceMinutes,
      })
      .ToList();

  private static List<BillingSettlementReportRowDto> BuildByPosition(List<SettlementReportSourceRow> rows) =>
    rows
      .GroupBy(r => r.PositionName)
      .OrderBy(g => g.Key)
      .Select(g => new BillingSettlementReportRowDto
      {
        Label = g.Key,
        PositionName = g.Key,
        UserCount = g.Select(x => x.UserId).Distinct().Count(),
        LoggedMinutes = g.Sum(x => x.LoggedMinutes),
        BalanceMinutes = g.Sum(x => x.BalanceMinutes),
      })
      .ToList();

  private static List<BillingSettlementReportRowDto> BuildByContractType(List<SettlementReportSourceRow> rows) =>
    rows
      .Where(r => r.ContractType.HasValue)
      .GroupBy(r => r.ContractType!.Value)
      .OrderBy(g => g.Key)
      .Select(g => new BillingSettlementReportRowDto
      {
        Label = g.Key.ToString(),
        ContractType = g.Key,
        UserCount = g.Select(x => x.UserId).Distinct().Count(),
        ExpectedMinutes = g.Sum(x => x.ExpectedMinutes),
        LoggedMinutes = g.Sum(x => x.LoggedMinutes),
      })
      .ToList();

  private static List<BillingSettlementReportRowDto> BuildOvertime(List<SettlementReportSourceRow> rows) =>
    rows
      .Where(r => r.BalanceMinutes > 0)
      .OrderByDescending(r => r.BalanceMinutes)
      .Select(r => new BillingSettlementReportRowDto
      {
        Label = r.DisplayName,
        UserDisplayName = r.DisplayName,
        ExpectedMinutes = r.ExpectedMinutes,
        LoggedMinutes = r.LoggedMinutes,
        LeaveDays = r.LeaveDays,
        BalanceMinutes = r.BalanceMinutes,
      })
      .ToList();

  private static List<BillingSettlementReportRowDto> BuildUndertime(List<SettlementReportSourceRow> rows) =>
    rows
      .Where(r => r.BalanceMinutes < 0)
      .OrderBy(r => r.BalanceMinutes)
      .Select(r => new BillingSettlementReportRowDto
      {
        Label = r.DisplayName,
        UserDisplayName = r.DisplayName,
        ExpectedMinutes = r.ExpectedMinutes,
        LoggedMinutes = r.LoggedMinutes,
        LeaveDays = r.LeaveDays,
        BalanceMinutes = r.BalanceMinutes,
      })
      .ToList();

  private sealed class SettlementReportSourceRow
  {
    public Guid UserId { get; init; }
    public string UserFirstName { get; init; } = string.Empty;
    public string UserLastName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string PositionName { get; init; } = "—";
    public ContractType? ContractType { get; init; }
    public int ExpectedMinutes { get; init; }
    public int LoggedMinutes { get; init; }
    public decimal LeaveDays { get; init; }
    public int BalanceMinutes { get; init; }

    public string DisplayName
    {
      get
      {
        var name = $"{UserFirstName} {UserLastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? UserEmail : $"{name} ({UserEmail})";
      }
    }
  }
}

public class GetBillingLeaveReportHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetBillingLeaveReportQuery, BillingLeaveReportResultDto>
{
  public async Task<Result<BillingLeaveReportResultDto>> Handle(
    GetBillingLeaveReportQuery query,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanViewBillingReports(userAccessor))
      return Result.Forbidden();

    var year = query.Year == 0
      ? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).Year
      : query.Year;

    var statuses = query.Statuses is { Count: > 0 }
      ? query.Statuses
      : [LeaveRequestStatus.Approved];

    return query.GroupingMode switch
    {
      BillingLeaveReportGroupingMode.ByPerson => await BuildByPersonAsync(
        context,
        year,
        query,
        statuses,
        cancellationToken),
      BillingLeaveReportGroupingMode.ByLeaveType => await BuildByLeaveTypeAsync(
        context,
        year,
        query,
        statuses,
        cancellationToken),
      BillingLeaveReportGroupingMode.LeaveCalendar => await BuildLeaveCalendarAsync(
        context,
        year,
        query,
        statuses,
        cancellationToken),
      _ => await BuildByPersonAsync(context, year, query, statuses, cancellationToken),
    };
  }

  private static async Task<Result<BillingLeaveReportResultDto>> BuildByPersonAsync(
    ApplicationDbContext context,
    int year,
    GetBillingLeaveReportQuery query,
    IReadOnlyList<LeaveRequestStatus> statuses,
    CancellationToken cancellationToken)
  {
    var usersQuery = context.Users.AsNoTracking().Where(u => !u.Deactivated);
    if (query.UserIds is { Count: > 0 })
      usersQuery = usersQuery.Where(u => query.UserIds.Contains(u.Id));

    var users = await usersQuery
      .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .ToListAsync(cancellationToken);

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var calculationDate = year == today.Year ? today : new DateOnly(year, 12, 31);
    var rows = new List<BillingLeaveReportRowDto>();

    foreach (var user in users)
    {
      var balance = await LeaveBalanceUtils.CalculateAsync(
        context,
        user.Id,
        year,
        calculationDate,
        cancellationToken);
      if (balance is null)
        continue;

      var name = $"{user.FirstName} {user.LastName}".Trim();
      rows.Add(new BillingLeaveReportRowDto
      {
        UserId = user.Id,
        UserDisplayName = string.IsNullOrWhiteSpace(name) ? user.Email : $"{name} ({user.Email})",
        EntitledDays = balance.EntitledDays,
        UsedDays = balance.UsedDays,
        RemainingDays = balance.RemainingDays,
      });
    }

    return Result.Success(new BillingLeaveReportResultDto
    {
      GroupingMode = BillingLeaveReportGroupingMode.ByPerson,
      Year = year,
      Rows = rows,
    });
  }

  private static async Task<Result<BillingLeaveReportResultDto>> BuildByLeaveTypeAsync(
    ApplicationDbContext context,
    int year,
    GetBillingLeaveReportQuery query,
    IReadOnlyList<LeaveRequestStatus> statuses,
    CancellationToken cancellationToken)
  {
    var yearStart = new DateOnly(year, 1, 1);
    var yearEnd = new DateOnly(year, 12, 31);

    var requestsQuery = context.LeaveRequests.AsNoTracking()
      .Where(r => statuses.Contains(r.Status)
                  && r.StartDate <= yearEnd
                  && r.EndDate >= yearStart);

    if (query.UserIds is { Count: > 0 })
      requestsQuery = requestsQuery.Where(r => query.UserIds.Contains(r.UserId));

    if (query.LeaveTypeIds is { Count: > 0 })
      requestsQuery = requestsQuery.Where(r => query.LeaveTypeIds.Contains(r.LeaveTypeId));

    var rawRows = await (
      from request in requestsQuery
      join leaveType in context.LeaveTypes.AsNoTracking() on request.LeaveTypeId equals leaveType.Id
      select new
      {
        leaveType.Name,
        request.StartDate,
        request.EndDate,
        request.DayPortion,
      }).ToListAsync(cancellationToken);

    var rows = rawRows
      .GroupBy(r => r.Name)
      .OrderBy(g => g.Key)
      .Select(g => new BillingLeaveReportRowDto
      {
        LeaveTypeName = g.Key,
        TotalDays = decimal.Round(
          g.Sum(r => LeaveRequestsUtils.CalculateDays(r.StartDate, r.EndDate, r.DayPortion)),
          1),
        RequestCount = g.Count(),
      })
      .ToList();

    return Result.Success(new BillingLeaveReportResultDto
    {
      GroupingMode = BillingLeaveReportGroupingMode.ByLeaveType,
      Year = year,
      Rows = rows,
    });
  }

  private static async Task<Result<BillingLeaveReportResultDto>> BuildLeaveCalendarAsync(
    ApplicationDbContext context,
    int year,
    GetBillingLeaveReportQuery query,
    IReadOnlyList<LeaveRequestStatus> statuses,
    CancellationToken cancellationToken)
  {
    var yearStart = new DateOnly(year, 1, 1);
    var yearEnd = new DateOnly(year, 12, 31);

    var requestsQuery = context.LeaveRequests.AsNoTracking()
      .Where(r => statuses.Contains(r.Status)
                  && r.StartDate <= yearEnd
                  && r.EndDate >= yearStart);

    if (query.UserIds is { Count: > 0 })
      requestsQuery = requestsQuery.Where(r => query.UserIds.Contains(r.UserId));

    if (query.LeaveTypeIds is { Count: > 0 })
      requestsQuery = requestsQuery.Where(r => query.LeaveTypeIds.Contains(r.LeaveTypeId));

    var rawRows = await (
      from request in requestsQuery
      join user in context.Users.AsNoTracking() on request.UserId equals user.Id
      join leaveType in context.LeaveTypes.AsNoTracking() on request.LeaveTypeId equals leaveType.Id
      orderby request.StartDate, user.LastName, user.FirstName
      select new
      {
        request.StartDate,
        request.EndDate,
        request.DayPortion,
        leaveType.Name,
        user.FirstName,
        user.LastName,
        user.Email,
      }).ToListAsync(cancellationToken);

    var rows = rawRows.Select(r =>
    {
      var name = $"{r.FirstName} {r.LastName}".Trim();
      return new BillingLeaveReportRowDto
      {
        UserDisplayName = string.IsNullOrWhiteSpace(name) ? r.Email : $"{name} ({r.Email})",
        LeaveTypeName = r.Name,
        DatesDisplay = LeaveRequestsUtils.FormatDates(r.StartDate, r.EndDate),
        Days = LeaveRequestsUtils.CalculateDays(r.StartDate, r.EndDate, r.DayPortion),
      };
    }).ToList();

    return Result.Success(new BillingLeaveReportResultDto
    {
      GroupingMode = BillingLeaveReportGroupingMode.LeaveCalendar,
      Year = year,
      Rows = rows,
    });
  }
}
