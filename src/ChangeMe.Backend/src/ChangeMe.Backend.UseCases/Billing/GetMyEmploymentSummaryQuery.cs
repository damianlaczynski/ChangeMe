using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetMyEmploymentSummaryQuery : IQuery<MyEmploymentSummaryDto?>;

public class GetMyEmploymentSummaryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetMyEmploymentSummaryQuery, MyEmploymentSummaryDto?>
{
  public async Task<Result<MyEmploymentSummaryDto?>> Handle(
    GetMyEmploymentSummaryQuery query,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingViewOwn);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var activeContract = await (
      from contract in context.EmploymentContracts.AsNoTracking()
      join position in context.Positions.AsNoTracking() on contract.PositionId equals position.Id
      where contract.UserId == userAccessor.UserId.Value
            && contract.StartDate <= today
            && (contract.EndDate == null || contract.EndDate >= today)
      orderby contract.StartDate descending
      select new { contract, position.Name })
      .FirstOrDefaultAsync(cancellationToken);

    if (activeContract is null)
      return Result.Success<MyEmploymentSummaryDto?>(null);

    return Result.Success<MyEmploymentSummaryDto?>(new MyEmploymentSummaryDto
    {
      PositionName = activeContract.Name,
      ContractType = activeContract.contract.ContractType,
      StartDate = activeContract.contract.StartDate,
      EndDate = activeContract.contract.EndDate,
      Fte = activeContract.contract.Fte,
      MonthlyHoursNormMinutes = activeContract.contract.MonthlyHoursNormMinutes,
    });
  }
}
