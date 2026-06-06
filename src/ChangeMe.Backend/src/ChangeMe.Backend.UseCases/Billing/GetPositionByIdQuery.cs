using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetPositionByIdQuery(Guid Id) : IQuery<PositionDetailsDto>;

public class GetPositionByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetPositionByIdQuery, PositionDetailsDto>
{
  public async Task<Result<PositionDetailsDto>> Handle(
    GetPositionByIdQuery query,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequireAnyPermission(
      userAccessor,
      PermissionCodes.BillingViewAny,
      PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var position = await context.Positions
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);
    if (position is null)
      return Result.NotFound();

    var contractCount = await context.EmploymentContracts
      .CountAsync(c => c.PositionId == position.Id, cancellationToken);
    var canManage = BillingUtils.CanManageEmployment(userAccessor);

    return Result.Success(new PositionDetailsDto
    {
      Id = position.Id,
      Name = position.Name,
      Department = string.IsNullOrEmpty(position.Department) ? null : position.Department,
      Description = string.IsNullOrEmpty(position.Description) ? null : position.Description,
      IsActive = position.IsActive,
      ContractCount = contractCount,
      CanManage = canManage,
      CanDelete = canManage && contractCount == 0,
    });
  }
}
