using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreateLeaveRequestCommand(
  Guid? UserId,
  Guid LeaveTypeId,
  DateOnly StartDate,
  DateOnly EndDate,
  LeaveDayPortion? DayPortion,
  string? Reason,
  bool Submit) : ICommand<LeaveRequestDetailsDto>;

public class CreateLeaveRequestHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<CreateLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    CreateLeaveRequestCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var targetUserId = command.UserId ?? userAccessor.UserId.Value;
    var isOwnRequest = targetUserId == userAccessor.UserId.Value;

    if (isOwnRequest)
    {
      if (!userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
        return Result.Forbidden();
    }
    else if (!userAccessor.HasPermission(PermissionCodes.BillingManageLeave))
    {
      return Result.Forbidden();
    }

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var createResult = await LeaveRequestWorkflow.PrepareLeaveRequestAsync(
      context,
      targetUserId,
      command.LeaveTypeId,
      command.StartDate,
      command.EndDate,
      command.DayPortion,
      command.Reason,
      excludeRequestId: null,
      today,
      cancellationToken);
    if (!createResult.IsSuccess)
      return createResult.Map();

    var leaveType = await context.LeaveTypes
      .AsNoTracking()
      .FirstAsync(lt => lt.Id == command.LeaveTypeId, cancellationToken);

    var request = createResult.Value;
    await context.LeaveRequests.AddAsync(request, cancellationToken);

    if (command.Submit)
    {
      var submitResult = request.Submit(timeProvider.GetUtcNow().UtcDateTime, leaveType.RequiresApproval);
      if (!submitResult.IsSuccess)
        return submitResult.Map();

      await AvailabilityLeaveSync.SyncIfApprovedAsync(context, request, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveRequestByIdQuery(request.Id), cancellationToken);
  }
}
