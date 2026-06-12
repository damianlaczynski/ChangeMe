using ChangeMe.Backend.UseCases.Notifications.Dtos;
using ChangeMe.Backend.UseCases.Notifications.Services;
using FastEndpoints;

namespace ChangeMe.Backend.UseCases.Notifications;

public sealed class GetNotificationsQuery : IQuery<NotificationListDto>
{
  public bool? IsRead { get; set; }

  [FromQuery]
  public PaginationParameters<NotificationDto> PaginationParameters { get; set; } =
    new PaginationParameters<NotificationDto>();
}

public class GetNotificationsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  NotificationRetentionPolicy retentionPolicy) : IQueryHandler<GetNotificationsQuery, NotificationListDto>
{
  public async ValueTask<Result<NotificationListDto>> Handle(
    GetNotificationsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result<NotificationListDto>.Unauthorized();

    var notificationsQuery = retentionPolicy.ApplyActiveFilter(
        context.Notifications
          .AsNoTracking()
          .Where(n => n.RecipientUserId == currentUserId));

    if (query.IsRead.HasValue)
      notificationsQuery = notificationsQuery.Where(n => n.IsRead == query.IsRead.Value);

    var projected = notificationsQuery.Select(n => new NotificationDto
    {
      Id = n.Id,
      IssueId = n.IssueId,
      EventType = n.EventType,
      IssueTitle = n.IssueTitle,
      Message = n.Message,
      Link = n.Link,
      CreatedAt = n.CreatedAt,
      IsRead = n.IsRead,
      ReadAt = n.ReadAt,
    });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedNotifications = await projected.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    var unreadNotificationsQuery = context.Notifications
      .AsNoTracking()
      .Where(n => n.RecipientUserId == currentUserId);

    var unreadCount = await retentionPolicy.ApplyActiveFilter(unreadNotificationsQuery)
      .CountAsync(n => !n.IsRead, cancellationToken);

    return Result.Success(new NotificationListDto
    {
      UnreadCount = unreadCount,
      Page = pagedNotifications
    });
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "CreatedAt" or _ => nameof(NotificationDto.CreatedAt)
    };
}
