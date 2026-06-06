using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetLeaveBalanceQuery(Guid UserId, int Year) : IQuery<LeaveBalanceDto>;

public class GetLeaveBalanceHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetLeaveBalanceQuery, LeaveBalanceDto>
{
  public async Task<Result<LeaveBalanceDto>> Handle(
    GetLeaveBalanceQuery query,
    CancellationToken cancellationToken)
  {
    var isOwnRequest = userAccessor.UserId == query.UserId;
    if (isOwnRequest)
    {
      if (!userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
        return Result.Forbidden();
    }
    else if (!BillingUtils.CanViewBillingReports(userAccessor))
    {
      return Result.Forbidden();
    }

    var userExists = await context.Users.AsNoTracking().AnyAsync(u => u.Id == query.UserId, cancellationToken);
    if (!userExists)
      return Result.NotFound();

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var calculationDate = query.Year == today.Year ? today : new DateOnly(query.Year, 12, 31);

    var balance = await LeaveBalanceUtils.CalculateAsync(
      context,
      query.UserId,
      query.Year,
      calculationDate,
      cancellationToken);
    if (balance is null)
      return Result.NotFound();

    return Result.Success(balance);
  }
}
