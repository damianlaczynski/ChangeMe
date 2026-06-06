using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreateLeaveTypeCommand(
  string Name,
  string Code,
  bool CountsAsPaid,
  bool UsesAllowance,
  bool RequiresApproval,
  bool IsActive) : ICommand<LeaveTypeDetailsDto>;

public class CreateLeaveTypeHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreateLeaveTypeCommand, LeaveTypeDetailsDto>
{
  public async Task<Result<LeaveTypeDetailsDto>> Handle(
    CreateLeaveTypeCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    var uniqueNameResult = await LeaveTypesUtils.EnsureUniqueLeaveTypeNameAsync(
      context,
      command.Name,
      excludeLeaveTypeId: null,
      cancellationToken);
    if (!uniqueNameResult.IsSuccess)
      return uniqueNameResult.Map();

    var uniqueCodeResult = await LeaveTypesUtils.EnsureUniqueLeaveTypeCodeAsync(
      context,
      command.Code,
      excludeLeaveTypeId: null,
      cancellationToken);
    if (!uniqueCodeResult.IsSuccess)
      return uniqueCodeResult.Map();

    var leaveTypeResult = LeaveType.Create(
      command.Name,
      command.Code,
      command.CountsAsPaid,
      command.UsesAllowance,
      command.RequiresApproval,
      command.IsActive);
    if (!leaveTypeResult.IsSuccess)
      return leaveTypeResult.Map();

    await context.LeaveTypes.AddAsync(leaveTypeResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var detailsResult = await mediator.Send(new GetLeaveTypeByIdQuery(leaveTypeResult.Value.Id), cancellationToken);
    if (!detailsResult.IsSuccess)
      return detailsResult.Map();

    return Result.Created(detailsResult.Value, $"/billing/leave-types/{leaveTypeResult.Value.Id}");
  }
}
