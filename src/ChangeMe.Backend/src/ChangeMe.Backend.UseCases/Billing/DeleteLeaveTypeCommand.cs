using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record DeleteLeaveTypeCommand(Guid Id) : ICommand<bool>;

public class DeleteLeaveTypeHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeleteLeaveTypeCommand, bool>
{
  public async Task<Result<bool>> Handle(
    DeleteLeaveTypeCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    var leaveType = await context.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == command.Id, cancellationToken);
    if (leaveType is null)
      return Result.NotFound();

    if (leaveType.IsSeeded)
      return Result.Error(BillingConstraints.SeededLeaveTypeDeleteMessage);

    var hasRequests = await LeaveTypesUtils.HasLeaveRequestsAsync(context, leaveType.Id, cancellationToken);
    if (hasRequests)
      return Result.Error(BillingConstraints.LeaveTypeReferencedMessage);

    context.LeaveTypes.Remove(leaveType);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}
