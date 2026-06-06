using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record SubmitLeaveRequestCommand(Guid Id) : ICommand<LeaveRequestDetailsDto>;

public class SubmitLeaveRequestHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<SubmitLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    SubmitLeaveRequestCommand command,
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

    if (!detailsResult.Value.CanSubmit)
      return Result.Forbidden();

    var leaveType = await context.LeaveTypes
      .AsNoTracking()
      .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId, cancellationToken);
    if (leaveType is null)
      return Result.NotFound();

    var overlapResult = await LeaveRequestsUtils.EnsureNoLeaveOverlapAsync(
      context,
      request.UserId,
      request.StartDate,
      request.EndDate,
      request.Id,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    var submitResult = request.Submit(
      timeProvider.GetUtcNow().UtcDateTime,
      leaveType.RequiresApproval);
    if (!submitResult.IsSuccess)
      return submitResult.Map();

    await AvailabilityLeaveSync.SyncIfApprovedAsync(context, request, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveRequestByIdQuery(command.Id), cancellationToken);
  }
}
