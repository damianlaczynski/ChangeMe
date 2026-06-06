using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public class GetLeaveRequestsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetLeaveRequestsQuery, PaginationResult<LeaveRequestListItemDto>>
{
  public async Task<Result<PaginationResult<LeaveRequestListItemDto>>> Handle(
    GetLeaveRequestsQuery query,
    CancellationToken cancellationToken)
  {
    if (!LeaveRequestsUtils.CanViewLeaveRequests(userAccessor))
      return Result.Forbidden();

    var requestsQuery = context.LeaveRequests.AsNoTracking();

    if (query.Statuses is { Count: > 0 })
      requestsQuery = requestsQuery.Where(r => query.Statuses.Contains(r.Status));

    if (query.LeaveTypeIds is { Count: > 0 })
      requestsQuery = requestsQuery.Where(r => query.LeaveTypeIds.Contains(r.LeaveTypeId));

    if (query.UserIds is { Count: > 0 })
    {
      if (!LeaveRequestsUtils.CanFilterByUsers(userAccessor))
        return Result.Forbidden();

      requestsQuery = requestsQuery.Where(r => query.UserIds.Contains(r.UserId));
    }

    if (query.DateFrom.HasValue)
      requestsQuery = requestsQuery.Where(r => r.EndDate >= query.DateFrom.Value);

    if (query.DateTo.HasValue)
      requestsQuery = requestsQuery.Where(r => r.StartDate <= query.DateTo.Value);

    if (string.IsNullOrWhiteSpace(query.PaginationParameters.SortField)
        || query.PaginationParameters.SortField == PaginationParameters<LeaveRequestListItemDto>.DefaultSortField)
    {
      query.PaginationParameters.SortField = nameof(LeaveRequestListItemDto.SubmittedAt);
      query.PaginationParameters.Ascending = false;
    }

    var paged = await requestsQuery
      .ProjectLeaveRequestListRows(context)
      .ToLeaveRequestListPageAsync(query.PaginationParameters, cancellationToken);

    return Result.Success(paged);
  }
}
