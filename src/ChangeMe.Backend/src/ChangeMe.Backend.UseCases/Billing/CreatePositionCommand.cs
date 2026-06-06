using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreatePositionCommand(string Name, string? Department, string? Description, bool IsActive)
  : ICommand<PositionDetailsDto>;

public class CreatePositionHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreatePositionCommand, PositionDetailsDto>
{
  public async Task<Result<PositionDetailsDto>> Handle(
    CreatePositionCommand command,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var uniqueNameResult = await PositionsUtils.EnsureUniquePositionNameAsync(
      context,
      command.Name,
      excludePositionId: null,
      cancellationToken);
    if (!uniqueNameResult.IsSuccess)
      return uniqueNameResult.Map();

    var positionResult = Position.Create(
      command.Name,
      command.Department,
      command.Description,
      command.IsActive);
    if (!positionResult.IsSuccess)
      return positionResult.Map();

    await context.Positions.AddAsync(positionResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var detailsResult = await mediator.Send(new GetPositionByIdQuery(positionResult.Value.Id), cancellationToken);
    if (!detailsResult.IsSuccess)
      return detailsResult.Map();

    return Result.Created(detailsResult.Value, $"/billing/positions/{positionResult.Value.Id}");
  }
}
