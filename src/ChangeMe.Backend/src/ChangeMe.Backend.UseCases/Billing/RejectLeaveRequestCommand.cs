using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record RejectLeaveRequestCommand(Guid Id, string RejectReason) : ICommand<LeaveRequestDetailsDto>;

public class RejectLeaveRequestHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<RejectLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    RejectLeaveRequestCommand command,
    CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.BillingApproveLeave))
      return Result.Forbidden();

    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var request = await context.LeaveRequests
      .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);
    if (request is null)
      return Result.NotFound();

    var rejectResult = request.Reject(
      userAccessor.UserId.Value,
      command.RejectReason,
      timeProvider.GetUtcNow().UtcDateTime);
    if (!rejectResult.IsSuccess)
      return rejectResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveRequestByIdQuery(command.Id), cancellationToken);
  }
}
