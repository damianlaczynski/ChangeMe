using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record UpdatePositionCommand(
  Guid Id,
  string Name,
  string? Department,
  string? Description,
  bool IsActive) : ICommand<PositionDetailsDto>;

public class UpdatePositionHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdatePositionCommand, PositionDetailsDto>
{
  public async Task<Result<PositionDetailsDto>> Handle(
    UpdatePositionCommand command,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var position = await context.Positions.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);
    if (position is null)
      return Result.NotFound();

    var uniqueNameResult = await PositionsUtils.EnsureUniquePositionNameAsync(
      context,
      command.Name,
      command.Id,
      cancellationToken);
    if (!uniqueNameResult.IsSuccess)
      return uniqueNameResult.Map();

    var updateResult = position.Update(command.Name, command.Department, command.Description, command.IsActive);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetPositionByIdQuery(command.Id), cancellationToken);
  }
}
