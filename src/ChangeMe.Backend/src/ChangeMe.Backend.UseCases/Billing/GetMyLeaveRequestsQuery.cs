using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public class GetMyLeaveRequestsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetMyLeaveRequestsQuery, PaginationResult<LeaveRequestListItemDto>>
{
  public async Task<Result<PaginationResult<LeaveRequestListItemDto>>> Handle(
    GetMyLeaveRequestsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (!userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
      return Result.Forbidden();

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var requestsQuery = context.LeaveRequests
      .AsNoTracking()
      .Where(r => r.UserId == userAccessor.UserId.Value);

    if (!query.ShowAllYears)
    {
      var cutoff = today.AddDays(-30);
      requestsQuery = requestsQuery.Where(r =>
        r.EndDate >= today
        || ((r.Status == LeaveRequestStatus.Draft || r.Status == LeaveRequestStatus.Submitted)
            && r.StartDate >= cutoff));
    }

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
