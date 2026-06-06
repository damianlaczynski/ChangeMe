using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetUserEmploymentQuery(Guid Id) : IQuery<UserEmploymentDto>;

public class GetUserEmploymentHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetUserEmploymentQuery, UserEmploymentDto>
{
  public async Task<Result<UserEmploymentDto>> Handle(
    GetUserEmploymentQuery query,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequireAnyPermission(
      userAccessor,
      PermissionCodes.BillingViewAny,
      PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var userExistsResult = await EmploymentUtils.EnsureUserExistsAsync(context, query.Id, cancellationToken);
    if (!userExistsResult.IsSuccess)
      return userExistsResult.Map();

    var canManage = BillingUtils.CanManageEmployment(userAccessor);
    var profile = await context.EmploymentProfiles
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.UserId == query.Id, cancellationToken);

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var contracts = await (
      from contract in context.EmploymentContracts.AsNoTracking()
      join position in context.Positions.AsNoTracking() on contract.PositionId equals position.Id
      where contract.UserId == query.Id
      orderby contract.StartDate descending, contract.CreatedAt descending
      select new { contract, position.Name })
      .ToListAsync(cancellationToken);

    var contractDtos = contracts
      .Select(item => EmploymentUtils.MapContractListItem(item.contract, item.Name, today))
      .ToList();

    return Result.Success(new UserEmploymentDto
    {
      Profile = EmploymentUtils.MapProfile(profile, canManage),
      Contracts = contractDtos,
      IsExpandedByDefault = contractDtos.Count > 0,
    });
  }
}
