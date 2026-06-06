using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public sealed class GetSettlementPeriodsQuery : IQuery<IReadOnlyList<SettlementPeriodListItemDto>>;

public class GetSettlementPeriodsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetSettlementPeriodsQuery, IReadOnlyList<SettlementPeriodListItemDto>>
{
  public async Task<Result<IReadOnlyList<SettlementPeriodListItemDto>>> Handle(
    GetSettlementPeriodsQuery query,
    CancellationToken cancellationToken)
  {
    if (!SettlementsUtils.CanViewSettlements(userAccessor))
      return Result.Forbidden();

    var periods = await context.SettlementPeriods
      .AsNoTracking()
      .OrderByDescending(p => p.Year)
      .ThenByDescending(p => p.Month)
      .ToListAsync(cancellationToken);

    var result = periods.Select(p => new SettlementPeriodListItemDto
    {
      Id = p.Id,
      Year = p.Year,
      Month = p.Month,
      Label = SettlementsUtils.FormatPeriodLabel(p.Year, p.Month),
      Status = p.Status,
    }).ToList();

    return Result.Success<IReadOnlyList<SettlementPeriodListItemDto>>(result);
  }
}

public record GetSettlementPeriodByIdQuery(Guid Id) : IQuery<SettlementPeriodDetailsDto>;

public class GetSettlementPeriodByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetSettlementPeriodByIdQuery, SettlementPeriodDetailsDto>
{
  public async Task<Result<SettlementPeriodDetailsDto>> Handle(
    GetSettlementPeriodByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (!SettlementsUtils.CanViewSettlements(userAccessor))
      return Result.Forbidden();

    var period = await context.SettlementPeriods
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);
    if (period is null)
      return Result.NotFound();

    var canManage = BillingUtils.CanManageSettlements(userAccessor);
    var canRecalculate = canManage && period.Status == SettlementPeriodStatus.Open;

    var settlements = await LoadUserSettlementListItemsAsync(
      context,
      period,
      canRecalculate,
      cancellationToken);

    var lastCalculatedAt = settlements.Count > 0
      ? settlements.Max(s => s.LastCalculatedAt)
      : (DateTime?)null;

    var closedByDisplayName = await SettlementsUtils.GetClosedByDisplayNameAsync(
      context,
      period.ClosedByUserId,
      cancellationToken);

    return Result.Success(new SettlementPeriodDetailsDto
    {
      Id = period.Id,
      Year = period.Year,
      Month = period.Month,
      Label = SettlementsUtils.FormatPeriodLabel(period.Year, period.Month),
      Status = period.Status,
      ClosedAt = period.ClosedAt,
      ClosedByDisplayName = closedByDisplayName,
      LastCalculatedAt = lastCalculatedAt,
      CanManage = canManage,
      UserSettlements = settlements,
    });
  }

  internal static async Task<List<UserSettlementListItemDto>> LoadUserSettlementListItemsAsync(
    ApplicationDbContext context,
    SettlementPeriod period,
    bool canRecalculate,
    CancellationToken cancellationToken)
  {
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
      orderby user.LastName, user.FirstName
      select new
      {
        settlement,
        user.FirstName,
        user.LastName,
        user.Email,
        PositionName = position != null ? position.Name : "—",
        ContractType = contract != null ? (ContractType?)contract.ContractType : null,
      }).ToListAsync(cancellationToken);

    return rows.Select(row =>
    {
      var name = $"{row.FirstName} {row.LastName}".Trim();
      var displayName = string.IsNullOrWhiteSpace(name) ? row.Email : $"{name} ({row.Email})";
      return SettlementsUtils.MapListItem(
        row.settlement,
        displayName,
        row.PositionName,
        row.ContractType,
        canRecalculate);
    }).ToList();
  }
}

public record GetUserSettlementByIdQuery(Guid Id) : IQuery<UserSettlementDetailsDto>;

public class GetUserSettlementByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetUserSettlementByIdQuery, UserSettlementDetailsDto>
{
  public async Task<Result<UserSettlementDetailsDto>> Handle(
    GetUserSettlementByIdQuery query,
    CancellationToken cancellationToken)
  {
    var settlement = await context.UserSettlements
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken);
    if (settlement is null)
      return Result.NotFound();

    var period = await context.SettlementPeriods
      .AsNoTracking()
      .FirstAsync(p => p.Id == settlement.SettlementPeriodId, cancellationToken);

    var isOwnClosed = userAccessor.UserId == settlement.UserId
                      && period.Status == SettlementPeriodStatus.Closed
                      && userAccessor.HasPermission(Domain.Authorization.PermissionCodes.BillingViewOwn);

    if (!SettlementsUtils.CanViewSettlements(userAccessor) && !isOwnClosed)
      return Result.Forbidden();

    if (isOwnClosed && !SettlementsUtils.CanViewSettlements(userAccessor)
        && period.Status != SettlementPeriodStatus.Closed)
      return Result.Forbidden();

    var calculation = await SettlementCalculationUtils.CalculateForUserAsync(
      context,
      settlement.UserId,
      period.Year,
      period.Month,
      cancellationToken);

    var userDisplayName = await EmploymentUtils.GetUserDisplayNameAsync(
      context,
      settlement.UserId,
      cancellationToken);

    UserSettlementContractSummaryDto? contractSummary = null;
    if (calculation.ContractId is not null
        && calculation.ContractStartDate is not null
        && calculation.ContractType is not null
        && calculation.ContractFte is not null)
    {
      contractSummary = new UserSettlementContractSummaryDto
      {
        ContractId = calculation.ContractId.Value,
        PositionName = calculation.PositionName,
        ContractType = calculation.ContractType.Value,
        StartDate = calculation.ContractStartDate.Value,
        EndDate = calculation.ContractEndDate,
        Fte = calculation.ContractFte.Value,
        MonthlyHoursNormMinutes = calculation.MonthlyHoursNormMinutes,
      };
    }

    var canRecalculate = BillingUtils.CanManageSettlements(userAccessor)
                         && period.Status == SettlementPeriodStatus.Open;

    return Result.Success(new UserSettlementDetailsDto
    {
      Id = settlement.Id,
      SettlementPeriodId = period.Id,
      Year = period.Year,
      Month = period.Month,
      PeriodLabel = SettlementsUtils.FormatPeriodLabel(period.Year, period.Month),
      UserId = settlement.UserId,
      UserDisplayName = userDisplayName,
      Contract = contractSummary,
      ExpectedTime = new UserSettlementExpectedTimeDto
      {
        MonthlyHoursNormMinutes = calculation.MonthlyHoursNormMinutes,
        ProrationNote = SettlementCalculationUtils.BuildProrationNote(calculation),
        PaidLeaveMinutesDeducted = calculation.PaidLeaveMinutesDeducted,
        ExpectedMinutes = settlement.ExpectedMinutes,
      },
      LoggedMinutes = settlement.LoggedMinutes,
      LeaveItems = calculation.LeaveItems
        .Select(item => new UserSettlementLeaveItemDto
        {
          LeaveTypeName = item.LeaveTypeName,
          DatesDisplay = LeaveRequestsUtils.FormatDates(item.StartDate, item.EndDate),
          Days = item.Days,
        })
        .ToList(),
      BalanceMinutes = settlement.BalanceMinutes,
      BalanceLabel = SettlementsUtils.FormatBalanceLabel(settlement.BalanceMinutes),
      CanRecalculate = canRecalculate,
    });
  }
}

public sealed class GetMySettlementsQuery : IQuery<IReadOnlyList<MySettlementListItemDto>>;

public class GetMySettlementsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetMySettlementsQuery, IReadOnlyList<MySettlementListItemDto>>
{
  public async Task<Result<IReadOnlyList<MySettlementListItemDto>>> Handle(
    GetMySettlementsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (!userAccessor.HasPermission(Domain.Authorization.PermissionCodes.BillingViewOwn))
      return Result.Forbidden();

    var rows = await (
      from settlement in context.UserSettlements.AsNoTracking()
      join period in context.SettlementPeriods.AsNoTracking() on settlement.SettlementPeriodId equals period.Id
      where settlement.UserId == userAccessor.UserId.Value
            && period.Status == SettlementPeriodStatus.Closed
      orderby period.Year descending, period.Month descending
      select new { settlement, period }).ToListAsync(cancellationToken);

    var settlements = rows.Select(row => new MySettlementListItemDto
    {
      Id = row.settlement.Id,
      SettlementPeriodId = row.period.Id,
      Year = row.period.Year,
      Month = row.period.Month,
      PeriodLabel = SettlementsUtils.FormatPeriodLabel(row.period.Year, row.period.Month),
      ExpectedMinutes = row.settlement.ExpectedMinutes,
      LoggedMinutes = row.settlement.LoggedMinutes,
      LeaveDays = row.settlement.LeaveDays,
      BalanceMinutes = row.settlement.BalanceMinutes,
    }).ToList();

    return Result.Success<IReadOnlyList<MySettlementListItemDto>>(settlements);
  }
}

public class GetSettlementOperationHistoryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetSettlementOperationHistoryQuery, PaginationResult<SettlementOperationLogListItemDto>>
{
  public async Task<Result<PaginationResult<SettlementOperationLogListItemDto>>> Handle(
    GetSettlementOperationHistoryQuery query,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanViewBillingReports(userAccessor))
      return Result.Forbidden();

    var entriesQuery = context.SettlementOperationLogEntries.AsNoTracking();

    if (query.SettlementPeriodId.HasValue)
      entriesQuery = entriesQuery.Where(e => e.SettlementPeriodId == query.SettlementPeriodId.Value);

    query.PaginationParameters.SortField = nameof(SettlementOperationLogEntry.Timestamp);
    query.PaginationParameters.Ascending = false;

    var entryParameters = PaginationParameters<SettlementOperationLogEntry>.Create(
      query.PaginationParameters.PageNumber,
      query.PaginationParameters.PageSize,
      nameof(SettlementOperationLogEntry.Timestamp),
      false);

    var pagedEntries = await entriesQuery.ToPaginationResultAsync<SettlementOperationLogEntry, SettlementOperationLogEntry>(
      x => x,
      entryParameters,
      cancellationToken);

    var actorIds = pagedEntries.Items.Select(e => e.ActorUserId).Distinct().ToList();
    var targetIds = pagedEntries.Items
      .Where(e => e.TargetUserId.HasValue)
      .Select(e => e.TargetUserId!.Value)
      .Distinct()
      .ToList();

    var userIds = actorIds.Union(targetIds).ToList();
    var users = await context.Users
      .AsNoTracking()
      .Where(u => userIds.Contains(u.Id))
      .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
      .ToListAsync(cancellationToken);

    string FormatUser(Guid userId)
    {
      var user = users.First(u => u.Id == userId);
      var name = $"{user.FirstName} {user.LastName}".Trim();
      return string.IsNullOrWhiteSpace(name) ? user.Email : $"{name} ({user.Email})";
    }

    var items = pagedEntries.Items.Select(entry => new SettlementOperationLogListItemDto
    {
      Id = entry.Id,
      Timestamp = entry.Timestamp,
      ActorDisplayName = FormatUser(entry.ActorUserId),
      Operation = entry.Operation,
      PeriodLabel = SettlementsUtils.FormatPeriodLabel(entry.PeriodYear, entry.PeriodMonth),
      UserDisplayName = entry.TargetUserId.HasValue ? FormatUser(entry.TargetUserId.Value) : null,
    }).ToList();

    return Result.Success(PaginationResult<SettlementOperationLogListItemDto>.Create(
      items,
      pagedEntries.TotalCount,
      query.PaginationParameters));
  }
}
