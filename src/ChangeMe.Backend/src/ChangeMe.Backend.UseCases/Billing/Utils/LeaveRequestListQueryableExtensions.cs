using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal sealed class LeaveRequestListRow
{
  public required LeaveRequest Request { get; init; }
  public required string UserFirstName { get; init; }
  public required string UserLastName { get; init; }
  public required string Email { get; init; }
  public required string LeaveTypeName { get; init; }
}

internal static class LeaveRequestListQueryableExtensions
{
  public static IQueryable<LeaveRequestListRow> ProjectLeaveRequestListRows(
    this IQueryable<LeaveRequest> requestsQuery,
    ApplicationDbContext context) =>
    from request in requestsQuery
    join user in context.Users.AsNoTracking() on request.UserId equals user.Id
    join leaveType in context.LeaveTypes.AsNoTracking() on request.LeaveTypeId equals leaveType.Id
    select new LeaveRequestListRow
    {
      Request = request,
      UserFirstName = user.FirstName,
      UserLastName = user.LastName,
      Email = user.Email,
      LeaveTypeName = leaveType.Name,
    };

  public static async Task<PaginationResult<LeaveRequestListItemDto>> ToLeaveRequestListPageAsync(
    this IQueryable<LeaveRequestListRow> rowsQuery,
    PaginationParameters<LeaveRequestListItemDto> parameters,
    CancellationToken cancellationToken)
  {
    parameters.SortField = MapSortField(parameters.SortField);

    var totalCount = await rowsQuery.CountAsync(cancellationToken);
    if (totalCount == 0)
      return PaginationResult<LeaveRequestListItemDto>.Create([], 0, parameters);

    var sorted = ApplySorting(rowsQuery, parameters);
    var page = await sorted
      .Skip((parameters.PageNumber - 1) * parameters.PageSize)
      .Take(parameters.PageSize)
      .ToListAsync(cancellationToken);

    var items = page
      .Select(row => LeaveRequestsUtils.MapListItem(
        row.Request,
        FormatUserReference(row.UserFirstName, row.UserLastName, row.Email),
        row.LeaveTypeName))
      .ToList();

    return PaginationResult<LeaveRequestListItemDto>.Create(items, totalCount, parameters);
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      nameof(LeaveRequestListItemDto.UserDisplayName) or "User" => "User",
      nameof(LeaveRequestListItemDto.LeaveTypeName) or "LeaveType" => nameof(LeaveRequestListItemDto.LeaveTypeName),
      nameof(LeaveRequestListItemDto.DatesDisplay) or "Dates" => nameof(LeaveRequestListItemDto.StartDate),
      nameof(LeaveRequestListItemDto.Days) => nameof(LeaveRequestListItemDto.StartDate),
      nameof(LeaveRequestListItemDto.Status) => nameof(LeaveRequestListItemDto.Status),
      nameof(LeaveRequestListItemDto.SubmittedAt) => nameof(LeaveRequestListItemDto.SubmittedAt),
      _ => nameof(LeaveRequestListItemDto.SubmittedAt),
    };

  private static IQueryable<LeaveRequestListRow> ApplySorting(
    IQueryable<LeaveRequestListRow> queryable,
    PaginationParameters<LeaveRequestListItemDto> parameters) =>
    parameters.SortField switch
    {
      "User" => parameters.Ascending
        ? queryable
          .OrderBy(row => row.UserLastName)
          .ThenBy(row => row.UserFirstName)
          .ThenByDescending(row => row.Request.SubmittedAt)
        : queryable
          .OrderByDescending(row => row.UserLastName)
          .ThenByDescending(row => row.UserFirstName)
          .ThenByDescending(row => row.Request.SubmittedAt),
      nameof(LeaveRequestListItemDto.LeaveTypeName) => parameters.Ascending
        ? queryable.OrderBy(row => row.LeaveTypeName).ThenByDescending(row => row.Request.SubmittedAt)
        : queryable.OrderByDescending(row => row.LeaveTypeName).ThenByDescending(row => row.Request.SubmittedAt),
      nameof(LeaveRequestListItemDto.StartDate) => parameters.Ascending
        ? queryable.OrderBy(row => row.Request.StartDate).ThenByDescending(row => row.Request.SubmittedAt)
        : queryable.OrderByDescending(row => row.Request.StartDate).ThenByDescending(row => row.Request.SubmittedAt),
      nameof(LeaveRequestListItemDto.Status) => parameters.Ascending
        ? queryable.OrderBy(row => row.Request.Status).ThenByDescending(row => row.Request.SubmittedAt)
        : queryable.OrderByDescending(row => row.Request.Status).ThenByDescending(row => row.Request.SubmittedAt),
      _ => parameters.Ascending
        ? queryable.OrderBy(row => row.Request.SubmittedAt).ThenBy(row => row.Request.StartDate)
        : queryable
          .OrderByDescending(row => row.Request.SubmittedAt)
          .ThenBy(row => row.Request.StartDate),
    };

  private static string FormatUserReference(string firstName, string lastName, string email)
  {
    var name = $"{firstName} {lastName}".Trim();
    return string.IsNullOrWhiteSpace(name) ? email : $"{name} ({email})";
  }
}
