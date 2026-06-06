using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record UpdateLeaveRequestCommand(
  Guid Id,
  Guid LeaveTypeId,
  DateOnly StartDate,
  DateOnly EndDate,
  LeaveDayPortion? DayPortion,
  string? Reason) : ICommand<LeaveRequestDetailsDto>;

public class UpdateLeaveRequestHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<UpdateLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    UpdateLeaveRequestCommand command,
    CancellationToken cancellationToken)
  {
    var request = await context.LeaveRequests
      .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);
    if (request is null)
      return Result.NotFound();

    var detailsResult = await GetLeaveRequestByIdHandler.MapDetailsAsync(
      context,
      userAccessor,
      request,
      cancellationToken);
    if (!detailsResult.IsSuccess)
      return detailsResult.Map();

    if (!detailsResult.Value.CanEdit)
      return Result.Forbidden();

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var startDateResult = LeaveRequestsUtils.ValidateStartDateWindow(command.StartDate, today);
    if (!startDateResult.IsSuccess)
      return startDateResult.Map();

    var leaveTypeResult = await LeaveRequestWorkflow.LoadActiveLeaveTypeAsync(
      context,
      command.LeaveTypeId,
      cancellationToken);
    if (!leaveTypeResult.IsSuccess)
      return leaveTypeResult.Map();

    var settingsResult = await LeaveRequestWorkflow.LoadBillingSettingsAsync(context, cancellationToken);
    if (!settingsResult.IsSuccess)
      return settingsResult.Map();

    var resolvedDayPortion = LeaveRequestsUtils.ResolveDayPortion(
      command.StartDate,
      command.EndDate,
      command.DayPortion,
      settingsResult.Value.AllowHalfDayLeave);

    var overlapResult = await LeaveRequestsUtils.EnsureNoLeaveOverlapAsync(
      context,
      request.UserId,
      command.StartDate,
      command.EndDate,
      command.Id,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    var updateResult = request.Update(
      command.LeaveTypeId,
      command.StartDate,
      command.EndDate,
      resolvedDayPortion,
      command.Reason);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveRequestByIdQuery(command.Id), cancellationToken);
  }
}
