using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Services;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreateAvailabilityEntryCommand(
  Guid? UserId,
  DateOnly StartDate,
  DateOnly EndDate,
  bool AllDay,
  TimeOnly? StartTime,
  TimeOnly? EndTime,
  AvailabilityStatus Status,
  string? Notes) : ICommand<AvailabilityEntryDto>;

public record UpdateAvailabilityEntryCommand(
  Guid Id,
  DateOnly StartDate,
  DateOnly EndDate,
  bool AllDay,
  TimeOnly? StartTime,
  TimeOnly? EndTime,
  AvailabilityStatus Status,
  string? Notes) : ICommand<AvailabilityEntryDto>;

public record DeleteAvailabilityEntryCommand(Guid Id) : ICommand<bool>;

public record SaveWeeklyRecurringPatternCommand(
  Guid? UserId,
  IReadOnlyList<WeeklyRecurringPatternDayDto> Days) : ICommand<WeeklyRecurringPatternDto>;

public record ResetWeeklyRecurringPatternCommand(Guid? UserId) : ICommand<WeeklyRecurringPatternDto>;

public class CreateAvailabilityEntryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  AvailabilityNotificationService availabilityNotificationService) : ICommandHandler<CreateAvailabilityEntryCommand, AvailabilityEntryDto>
{
  public async Task<Result<AvailabilityEntryDto>> Handle(
    CreateAvailabilityEntryCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var targetUserId = command.UserId ?? userAccessor.UserId.Value;
    if (!AvailabilityUtils.CanManageUserAvailability(userAccessor, targetUserId))
      return Result.Forbidden();

    var overlapResult = await AvailabilityUtils.EnsureNoManualOverlapAsync(
      context,
      targetUserId,
      command.StartDate,
      command.EndDate,
      command.AllDay,
      command.StartTime,
      command.EndTime,
      excludeEntryId: null,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    var createResult = AvailabilityEntry.CreateManual(
      targetUserId,
      command.StartDate,
      command.EndDate,
      command.AllDay,
      command.StartTime,
      command.EndTime,
      command.Status,
      command.Notes);
    if (!createResult.IsSuccess)
      return createResult.Map();

    await context.AvailabilityEntries.AddAsync(createResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    await availabilityNotificationService.NotifyManualEntryChangedAsync(
      createResult.Value,
      userAccessor.UserId.Value,
      cancellationToken);

    return Result.Success(AvailabilityUtils.MapEntry(createResult.Value));
  }
}

public class UpdateAvailabilityEntryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  AvailabilityNotificationService availabilityNotificationService) : ICommandHandler<UpdateAvailabilityEntryCommand, AvailabilityEntryDto>
{
  public async Task<Result<AvailabilityEntryDto>> Handle(
    UpdateAvailabilityEntryCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var entry = await context.AvailabilityEntries
      .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
    if (entry is null)
      return Result.NotFound();

    if (!AvailabilityUtils.CanManageUserAvailability(userAccessor, entry.UserId))
      return Result.Forbidden();

    var overlapResult = await AvailabilityUtils.EnsureNoManualOverlapAsync(
      context,
      entry.UserId,
      command.StartDate,
      command.EndDate,
      command.AllDay,
      command.StartTime,
      command.EndTime,
      excludeEntryId: entry.Id,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    var updateResult = entry.UpdateManual(
      command.StartDate,
      command.EndDate,
      command.AllDay,
      command.StartTime,
      command.EndTime,
      command.Status,
      command.Notes);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    await availabilityNotificationService.NotifyManualEntryChangedAsync(
      entry,
      userAccessor.UserId.Value,
      cancellationToken);

    return Result.Success(AvailabilityUtils.MapEntry(entry));
  }
}

public class DeleteAvailabilityEntryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  AvailabilityNotificationService availabilityNotificationService) : ICommandHandler<DeleteAvailabilityEntryCommand, bool>
{
  public async Task<Result<bool>> Handle(
    DeleteAvailabilityEntryCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var entry = await context.AvailabilityEntries
      .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
    if (entry is null)
      return Result.NotFound();

    if (entry.Source != AvailabilityEntrySource.Manual)
      return Result.Conflict("Only manual availability entries can be deleted.");

    if (!AvailabilityUtils.CanManageUserAvailability(userAccessor, entry.UserId))
      return Result.Forbidden();

    var entryUserId = entry.UserId;
    var entryId = entry.Id;
    var startDate = entry.StartDate;
    var endDate = entry.EndDate;
    var allDay = entry.AllDay;
    var startTime = entry.StartTime;
    var endTime = entry.EndTime;
    var status = entry.Status;
    var revisionAt = entry.UpdatedAt ?? entry.CreatedAt;

    context.AvailabilityEntries.Remove(entry);
    await context.SaveChangesAsync(cancellationToken);

    await availabilityNotificationService.NotifyManualEntryDeletedAsync(
      entryUserId,
      entryId,
      startDate,
      endDate,
      allDay,
      startTime,
      endTime,
      status,
      revisionAt,
      userAccessor.UserId.Value,
      cancellationToken);

    return Result.Success(true);
  }
}

public class SaveWeeklyRecurringPatternHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider,
  AvailabilityNotificationService availabilityNotificationService) : ICommandHandler<SaveWeeklyRecurringPatternCommand, WeeklyRecurringPatternDto>
{
  public async Task<Result<WeeklyRecurringPatternDto>> Handle(
    SaveWeeklyRecurringPatternCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var targetUserId = command.UserId ?? userAccessor.UserId.Value;
    if (!AvailabilityUtils.CanManageUserAvailability(userAccessor, targetUserId))
      return Result.Forbidden();

    var pattern = await context.WeeklyRecurringPatterns
      .Include(p => p.Days)
      .FirstOrDefaultAsync(p => p.UserId == targetUserId, cancellationToken);

    if (pattern is null)
    {
      pattern = WeeklyRecurringPattern.CreateEmpty(targetUserId);
      await context.WeeklyRecurringPatterns.AddAsync(pattern, cancellationToken);
    }

    var dayInputs = command.Days
      .Select(d => new WeeklyRecurringPatternDayInput(
        d.DayOfWeek,
        d.Enabled,
        d.StartTime,
        d.EndTime,
        d.Status))
      .ToList();

    var replaceResult = pattern.ReplaceDays(dayInputs);
    if (!replaceResult.IsSuccess)
      return replaceResult.Map();

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    await AvailabilityUtils.RegenerateRecurringEntriesAsync(context, pattern, today, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    await availabilityNotificationService.NotifyWeeklyPatternChangedAsync(
      pattern,
      userAccessor.UserId.Value,
      cancellationToken);

    return await GetUserWeeklyPatternHandler.LoadPatternAsync(
      context,
      userAccessor,
      targetUserId,
      cancellationToken);
  }
}

public class ResetWeeklyRecurringPatternHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider,
  AvailabilityNotificationService availabilityNotificationService) : ICommandHandler<ResetWeeklyRecurringPatternCommand, WeeklyRecurringPatternDto>
{
  public async Task<Result<WeeklyRecurringPatternDto>> Handle(
    ResetWeeklyRecurringPatternCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var targetUserId = command.UserId ?? userAccessor.UserId.Value;
    if (!AvailabilityUtils.CanManageUserAvailability(userAccessor, targetUserId))
      return Result.Forbidden();

    var settings = await context.BillingSettings
      .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
    if (settings is null)
      return Result.NotFound();

    var fte = await AvailabilityUtils.GetActiveContractFteAsync(
      context,
      targetUserId,
      DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime),
      cancellationToken) ?? 1m;

    var pattern = await context.WeeklyRecurringPatterns
      .Include(p => p.Days)
      .FirstOrDefaultAsync(p => p.UserId == targetUserId, cancellationToken);

    if (pattern is null)
    {
      pattern = WeeklyRecurringPattern.CreateDefault(targetUserId, settings, fte);
      await context.WeeklyRecurringPatterns.AddAsync(pattern, cancellationToken);
    }
    else
    {
      var applyResult = pattern.ApplyOrganizationDefaults(settings, fte);
      if (!applyResult.IsSuccess)
        return applyResult.Map();
    }

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    await AvailabilityUtils.RegenerateRecurringEntriesAsync(context, pattern, today, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    await availabilityNotificationService.NotifyWeeklyPatternChangedAsync(
      pattern,
      userAccessor.UserId.Value,
      cancellationToken);

    return await GetUserWeeklyPatternHandler.LoadPatternAsync(
      context,
      userAccessor,
      targetUserId,
      cancellationToken);
  }
}
