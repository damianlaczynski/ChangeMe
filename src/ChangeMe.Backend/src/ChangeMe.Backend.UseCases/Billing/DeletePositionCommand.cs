using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record DeletePositionCommand(Guid Id) : ICommand<bool>;

public class DeletePositionHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeletePositionCommand, bool>
{
  public async Task<Result<bool>> Handle(
    DeletePositionCommand command,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var position = await context.Positions.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
    if (position is null)
      return Result.NotFound();

    var hasContracts = await context.EmploymentContracts
      .AnyAsync(c => c.PositionId == position.Id, cancellationToken);
    if (hasContracts)
      return Result.Conflict(BillingConstraints.PositionReferencedMessage);

    context.Positions.Remove(position);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
