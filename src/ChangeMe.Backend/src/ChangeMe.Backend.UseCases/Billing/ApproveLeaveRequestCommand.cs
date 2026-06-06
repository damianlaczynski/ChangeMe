using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record ApproveLeaveRequestCommand(Guid Id) : ICommand<LeaveRequestDetailsDto>;

public class ApproveLeaveRequestHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<ApproveLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    ApproveLeaveRequestCommand command,
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

    var approveResult = request.Approve(userAccessor.UserId.Value, timeProvider.GetUtcNow().UtcDateTime);
    if (!approveResult.IsSuccess)
      return approveResult.Map();

    await AvailabilityLeaveSync.SyncIfApprovedAsync(context, request, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveRequestByIdQuery(command.Id), cancellationToken);
  }
}
