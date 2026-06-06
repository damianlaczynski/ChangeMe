using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetLeaveTypesQuery : IQuery<IReadOnlyList<LeaveTypeListItemDto>>;

public class GetLeaveTypesHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetLeaveTypesQuery, IReadOnlyList<LeaveTypeListItemDto>>
{
  public async Task<Result<IReadOnlyList<LeaveTypeListItemDto>>> Handle(
    GetLeaveTypesQuery query,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanViewBillingReports(userAccessor))
      return Result.Forbidden();

    var canManage = BillingUtils.CanManageSettlements(userAccessor);
    var leaveTypes = await context.LeaveTypes
      .AsNoTracking()
      .OrderBy(lt => lt.Name)
      .ToListAsync(cancellationToken);

    var items = leaveTypes
      .Select(lt => LeaveTypesUtils.MapListItem(lt, canManage))
      .ToList();

    foreach (var item in items.Where(i => i.CanDelete))
    {
      var canDelete = !await LeaveTypesUtils.HasLeaveRequestsAsync(context, item.Id, cancellationToken);
      item.CanDelete = canDelete;
    }

    return Result.Success<IReadOnlyList<LeaveTypeListItemDto>>(items);
  }
}
