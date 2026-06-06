using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetLeaveTypeByIdQuery(Guid Id) : IQuery<LeaveTypeDetailsDto>;

public class GetLeaveTypeByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetLeaveTypeByIdQuery, LeaveTypeDetailsDto>
{
  public async Task<Result<LeaveTypeDetailsDto>> Handle(
    GetLeaveTypeByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanViewBillingReports(userAccessor))
      return Result.Forbidden();

    var leaveType = await context.LeaveTypes
      .AsNoTracking()
      .FirstOrDefaultAsync(lt => lt.Id == query.Id, cancellationToken);
    if (leaveType is null)
      return Result.NotFound();

    var canManage = BillingUtils.CanManageSettlements(userAccessor);
    var canDelete = canManage
                    && !leaveType.IsSeeded
                    && !await LeaveTypesUtils.HasLeaveRequestsAsync(context, leaveType.Id, cancellationToken);

    return Result.Success(LeaveTypesUtils.MapDetails(leaveType, canManage, canDelete));
  }
}
