using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreateMyLeaveRequestCommand(
  Guid LeaveTypeId,
  DateOnly StartDate,
  DateOnly EndDate,
  LeaveDayPortion? DayPortion,
  string? Reason,
  bool Submit) : ICommand<LeaveRequestDetailsDto>;

public class CreateMyLeaveRequestHandler(IMediator mediator)
  : ICommandHandler<CreateMyLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public Task<Result<LeaveRequestDetailsDto>> Handle(
    CreateMyLeaveRequestCommand command,
    CancellationToken cancellationToken) =>
    mediator.Send(
      new CreateLeaveRequestCommand(
        UserId: null,
        command.LeaveTypeId,
        command.StartDate,
        command.EndDate,
        command.DayPortion,
        command.Reason,
        command.Submit),
      cancellationToken);
}
