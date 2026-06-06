using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetMyAvailabilityDayQuery(DateOnly Date) : IQuery<AvailabilityDayResultDto>;

public record GetUserAvailabilityDayQuery(Guid UserId, DateOnly Date) : IQuery<AvailabilityDayResultDto>;

public record SaveMyWeeklyPatternCommand(IReadOnlyList<WeeklyRecurringPatternDayDto> Days)
  : ICommand<WeeklyRecurringPatternDto>;

public record SaveUserWeeklyPatternCommand(Guid UserId, IReadOnlyList<WeeklyRecurringPatternDayDto> Days)
  : ICommand<WeeklyRecurringPatternDto>;

public record ResetMyWeeklyPatternCommand() : ICommand<WeeklyRecurringPatternDto>;

public record ResetUserWeeklyPatternCommand(Guid UserId) : ICommand<WeeklyRecurringPatternDto>;

public record CreateMyAvailabilityEntryCommand(
  DateOnly StartDate,
  DateOnly EndDate,
  bool AllDay,
  TimeOnly? StartTime,
  TimeOnly? EndTime,
  Domain.Aggregates.Billing.Enums.AvailabilityStatus Status,
  string? Notes) : ICommand<AvailabilityEntryDto>;

public record CreateUserAvailabilityEntryCommand(
  Guid UserId,
  DateOnly StartDate,
  DateOnly EndDate,
  bool AllDay,
  TimeOnly? StartTime,
  TimeOnly? EndTime,
  Domain.Aggregates.Billing.Enums.AvailabilityStatus Status,
  string? Notes) : ICommand<AvailabilityEntryDto>;

public class GetMyAvailabilityDayHandler(
  IMediator mediator,
  IUserAccessor userAccessor) : IQueryHandler<GetMyAvailabilityDayQuery, AvailabilityDayResultDto>
{
  public async Task<Result<AvailabilityDayResultDto>> Handle(
    GetMyAvailabilityDayQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    return await mediator.Send(
      new GetAvailabilityDayQuery(userAccessor.UserId.Value, query.Date),
      cancellationToken);
  }
}

public class GetUserAvailabilityDayHandler(IMediator mediator)
  : IQueryHandler<GetUserAvailabilityDayQuery, AvailabilityDayResultDto>
{
  public Task<Result<AvailabilityDayResultDto>> Handle(
    GetUserAvailabilityDayQuery query,
    CancellationToken cancellationToken) =>
    mediator.Send(new GetAvailabilityDayQuery(query.UserId, query.Date), cancellationToken);
}

public class SaveMyWeeklyPatternHandler(IMediator mediator)
  : ICommandHandler<SaveMyWeeklyPatternCommand, WeeklyRecurringPatternDto>
{
  public Task<Result<WeeklyRecurringPatternDto>> Handle(
    SaveMyWeeklyPatternCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(new SaveWeeklyRecurringPatternCommand(null, command.Days), cancellationToken);
}

public class SaveUserWeeklyPatternHandler(IMediator mediator)
  : ICommandHandler<SaveUserWeeklyPatternCommand, WeeklyRecurringPatternDto>
{
  public Task<Result<WeeklyRecurringPatternDto>> Handle(
    SaveUserWeeklyPatternCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(
      new SaveWeeklyRecurringPatternCommand(command.UserId, command.Days),
      cancellationToken);
}

public class ResetMyWeeklyPatternHandler(IMediator mediator)
  : ICommandHandler<ResetMyWeeklyPatternCommand, WeeklyRecurringPatternDto>
{
  public Task<Result<WeeklyRecurringPatternDto>> Handle(
    ResetMyWeeklyPatternCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(new ResetWeeklyRecurringPatternCommand(null), cancellationToken);
}

public class ResetUserWeeklyPatternHandler(IMediator mediator)
  : ICommandHandler<ResetUserWeeklyPatternCommand, WeeklyRecurringPatternDto>
{
  public Task<Result<WeeklyRecurringPatternDto>> Handle(
    ResetUserWeeklyPatternCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(new ResetWeeklyRecurringPatternCommand(command.UserId), cancellationToken);
}

public class CreateMyAvailabilityEntryHandler(IMediator mediator)
  : ICommandHandler<CreateMyAvailabilityEntryCommand, AvailabilityEntryDto>
{
  public Task<Result<AvailabilityEntryDto>> Handle(
    CreateMyAvailabilityEntryCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(
      new CreateAvailabilityEntryCommand(
        null,
        command.StartDate,
        command.EndDate,
        command.AllDay,
        command.StartTime,
        command.EndTime,
        command.Status,
        command.Notes),
      cancellationToken);
}

public class CreateUserAvailabilityEntryHandler(IMediator mediator)
  : ICommandHandler<CreateUserAvailabilityEntryCommand, AvailabilityEntryDto>
{
  public Task<Result<AvailabilityEntryDto>> Handle(
    CreateUserAvailabilityEntryCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(
      new CreateAvailabilityEntryCommand(
        command.UserId,
        command.StartDate,
        command.EndDate,
        command.AllDay,
        command.StartTime,
        command.EndTime,
        command.Status,
        command.Notes),
      cancellationToken);
}
