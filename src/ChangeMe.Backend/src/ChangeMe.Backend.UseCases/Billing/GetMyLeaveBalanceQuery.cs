using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public sealed class GetMyLeaveBalanceQuery : IQuery<LeaveBalanceDto>
{
  public int Year { get; set; }
}

public class GetMyLeaveBalanceHandler(
  IMediator mediator,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetMyLeaveBalanceQuery, LeaveBalanceDto>
{
  public async Task<Result<LeaveBalanceDto>> Handle(
    GetMyLeaveBalanceQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (!userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
      return Result.Forbidden();

    var year = query.Year == 0
      ? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).Year
      : query.Year;

    return await mediator.Send(new GetLeaveBalanceQuery(userAccessor.UserId.Value, year), cancellationToken);
  }
}
