using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record UpdateLeaveTypeCommand(
  Guid Id,
  string Name,
  string Code,
  bool CountsAsPaid,
  bool UsesAllowance,
  bool RequiresApproval,
  bool IsActive) : ICommand<LeaveTypeDetailsDto>;

public class UpdateLeaveTypeHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateLeaveTypeCommand, LeaveTypeDetailsDto>
{
  public async Task<Result<LeaveTypeDetailsDto>> Handle(
    UpdateLeaveTypeCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    var leaveType = await context.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == command.Id, cancellationToken);
    if (leaveType is null)
      return Result.NotFound();

    var uniqueNameResult = await LeaveTypesUtils.EnsureUniqueLeaveTypeNameAsync(
      context,
      command.Name,
      command.Id,
      cancellationToken);
    if (!uniqueNameResult.IsSuccess)
      return uniqueNameResult.Map();

    var uniqueCodeResult = await LeaveTypesUtils.EnsureUniqueLeaveTypeCodeAsync(
      context,
      command.Code,
      command.Id,
      cancellationToken);
    if (!uniqueCodeResult.IsSuccess)
      return uniqueCodeResult.Map();

    var updateResult = leaveType.Update(
      command.Name,
      command.Code,
      command.CountsAsPaid,
      command.UsesAllowance,
      command.RequiresApproval,
      command.IsActive);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetLeaveTypeByIdQuery(command.Id), cancellationToken);
  }
}
