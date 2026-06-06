using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreateSettlementPeriodCommand(int Year, int Month) : ICommand<SettlementPeriodDetailsDto>;

public class CreateSettlementPeriodHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<CreateSettlementPeriodCommand, SettlementPeriodDetailsDto>
{
  public async Task<Result<SettlementPeriodDetailsDto>> Handle(
    CreateSettlementPeriodCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (command.Month is < 1 or > 12)
    {
      return Result.Invalid(new ValidationError(
        nameof(command.Month),
        "Month must be between 1 and 12."));
    }

    var currentYear = timeProvider.GetUtcNow().UtcDateTime.Year;
    if (command.Year < currentYear - 2 || command.Year > currentYear + 2)
    {
      return Result.Invalid(new ValidationError(
        nameof(command.Year),
        $"Year must be between {currentYear - 2} and {currentYear + 2}."));
    }

    var duplicate = await context.SettlementPeriods
      .AnyAsync(p => p.Year == command.Year && p.Month == command.Month, cancellationToken);
    if (duplicate)
      return Result.Conflict(BillingConstraints.SettlementPeriodDuplicateMessage);

    var createResult = SettlementPeriod.Create(command.Year, command.Month);
    if (!createResult.IsSuccess)
      return createResult.Map();

    var period = createResult.Value;
    var calculatedAt = timeProvider.GetUtcNow().UtcDateTime;
    var monthStart = new DateOnly(period.Year, period.Month, 1);
    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

    await context.SettlementPeriods.AddAsync(period, cancellationToken);

    var userIds = await SettlementCalculationUtils.GetEligibleUserIdsAsync(
      context,
      monthStart,
      monthEnd,
      cancellationToken);

    foreach (var userId in userIds)
    {
      var calculation = await SettlementCalculationUtils.CalculateForUserAsync(
        context,
        userId,
        period.Year,
        period.Month,
        cancellationToken);

      await SettlementCalculationUtils.UpsertUserSettlementAsync(
        context,
        period.Id,
        userId,
        calculation,
        calculatedAt,
        cancellationToken);
    }

    await context.SettlementOperationLogEntries.AddAsync(
      SettlementOperationLogEntry.Create(
        period.Id,
        period.Year,
        period.Month,
        SettlementOperationType.Created,
        userAccessor.UserId.Value,
        calculatedAt),
      cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetSettlementPeriodByIdQuery(period.Id), cancellationToken);
  }
}

public record RecalculateAllSettlementsCommand(Guid SettlementPeriodId) : ICommand<SettlementPeriodDetailsDto>;

public class RecalculateAllSettlementsHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<RecalculateAllSettlementsCommand, SettlementPeriodDetailsDto>
{
  public async Task<Result<SettlementPeriodDetailsDto>> Handle(
    RecalculateAllSettlementsCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var period = await context.SettlementPeriods
      .FirstOrDefaultAsync(p => p.Id == command.SettlementPeriodId, cancellationToken);
    if (period is null)
      return Result.NotFound();

    if (period.Status == SettlementPeriodStatus.Closed)
      return Result.Conflict(BillingConstraints.SettlementPeriodClosedMessage);

    var calculatedAt = timeProvider.GetUtcNow().UtcDateTime;
    var monthStart = new DateOnly(period.Year, period.Month, 1);
    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

    var userIds = await SettlementCalculationUtils.GetEligibleUserIdsAsync(
      context,
      monthStart,
      monthEnd,
      cancellationToken);

    foreach (var userId in userIds)
    {
      var calculation = await SettlementCalculationUtils.CalculateForUserAsync(
        context,
        userId,
        period.Year,
        period.Month,
        cancellationToken);

      await SettlementCalculationUtils.UpsertUserSettlementAsync(
        context,
        period.Id,
        userId,
        calculation,
        calculatedAt,
        cancellationToken);
    }

    await context.SettlementOperationLogEntries.AddAsync(
      SettlementOperationLogEntry.Create(
        period.Id,
        period.Year,
        period.Month,
        SettlementOperationType.Recalculated,
        userAccessor.UserId.Value,
        calculatedAt),
      cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetSettlementPeriodByIdQuery(period.Id), cancellationToken);
  }
}

public record RecalculateUserSettlementCommand(Guid UserSettlementId) : ICommand<UserSettlementDetailsDto>;

public class RecalculateUserSettlementHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<RecalculateUserSettlementCommand, UserSettlementDetailsDto>
{
  public async Task<Result<UserSettlementDetailsDto>> Handle(
    RecalculateUserSettlementCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var settlement = await context.UserSettlements
      .FirstOrDefaultAsync(s => s.Id == command.UserSettlementId, cancellationToken);
    if (settlement is null)
      return Result.NotFound();

    var period = await context.SettlementPeriods
      .FirstAsync(p => p.Id == settlement.SettlementPeriodId, cancellationToken);

    if (period.Status == SettlementPeriodStatus.Closed)
      return Result.Conflict(BillingConstraints.SettlementPeriodClosedMessage);

    var calculatedAt = timeProvider.GetUtcNow().UtcDateTime;
    var calculation = await SettlementCalculationUtils.CalculateForUserAsync(
      context,
      settlement.UserId,
      period.Year,
      period.Month,
      cancellationToken);

    settlement.UpdateCalculation(
      calculation.ContractId,
      calculation.ExpectedMinutes,
      calculation.LoggedMinutes,
      calculation.LeaveDays,
      calculatedAt);

    await context.SettlementOperationLogEntries.AddAsync(
      SettlementOperationLogEntry.Create(
        period.Id,
        period.Year,
        period.Month,
        SettlementOperationType.Recalculated,
        userAccessor.UserId.Value,
        calculatedAt,
        settlement.UserId),
      cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetUserSettlementByIdQuery(settlement.Id), cancellationToken);
  }
}

public record CloseSettlementPeriodCommand(Guid SettlementPeriodId) : ICommand<SettlementPeriodDetailsDto>;

public class CloseSettlementPeriodHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<CloseSettlementPeriodCommand, SettlementPeriodDetailsDto>
{
  public async Task<Result<SettlementPeriodDetailsDto>> Handle(
    CloseSettlementPeriodCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var period = await context.SettlementPeriods
      .FirstOrDefaultAsync(p => p.Id == command.SettlementPeriodId, cancellationToken);
    if (period is null)
      return Result.NotFound();

    if (period.Status == SettlementPeriodStatus.Closed)
      return Result.Conflict(BillingConstraints.SettlementPeriodClosedMessage);

    var closedAt = timeProvider.GetUtcNow().UtcDateTime;
    var closeResult = period.Close(userAccessor.UserId.Value, closedAt);
    if (!closeResult.IsSuccess)
      return closeResult.Map();

    await context.SettlementOperationLogEntries.AddAsync(
      SettlementOperationLogEntry.Create(
        period.Id,
        period.Year,
        period.Month,
        SettlementOperationType.Closed,
        userAccessor.UserId.Value,
        closedAt),
      cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetSettlementPeriodByIdQuery(period.Id), cancellationToken);
  }
}
