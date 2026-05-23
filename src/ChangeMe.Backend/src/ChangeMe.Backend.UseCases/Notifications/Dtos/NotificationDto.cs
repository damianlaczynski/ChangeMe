using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;

namespace ChangeMe.Backend.UseCases.Notifications.Dtos;

public class NotificationDto
{
  public Guid Id { get; set; }
  public Guid IssueId { get; set; }
  public NotificationEventType EventType { get; set; }
  public string IssueTitle { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string Link { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public bool IsRead { get; set; }
  public DateTime? ReadAt { get; set; }
}

public class NotificationListDto
{
  public int UnreadCount { get; set; }
  public PaginationResult<NotificationDto> Page { get; set; } = PaginationResult<NotificationDto>.Empty();
}
