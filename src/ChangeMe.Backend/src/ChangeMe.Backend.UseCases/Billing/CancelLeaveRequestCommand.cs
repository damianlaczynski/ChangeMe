using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CancelLeaveRequestCommand(Guid Id) : ICommand<LeaveRequestDetailsDto>;

public class CancelLeaveRequestHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CancelLeaveRequestCommand, LeaveRequestDetailsDto>
{
  public async Task<Result<LeaveRequestDetailsDto>> Handle(
    CancelLeaveRequestCommand command,
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

    if (!detailsResult.Value.CanCancel)
      return Result.Forbidden();

    var previousStatus = request.Status;
    var asAdministrator = userAccessor.HasPermission(PermissionCodes.BillingManageLeave);
    var cancelResult = request.Cancel(asAdministrator);
    if (!cancelResult.IsSuccess)
      return cancelResult.Map();

    await AvailabilityLeaveSync.RemoveIfNeededAsync(context, request, previousStatus, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveRequestByIdQuery(command.Id), cancellationToken);
  }
}
