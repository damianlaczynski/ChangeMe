using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;

namespace ChangeMe.Backend.UseCases.Issues.Dtos;

public class IssueDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public IssueStatus Status { get; set; }
  public IssuePriority Priority { get; set; }
  public Guid CreatedBy { get; set; }
  public string? CreatedByName { get; set; }
  public Guid? AssignedToUserId { get; set; }
  public string? AssignedToUserName { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime LastActivityAt { get; set; }
  public bool IsWatchedByCurrentUser { get; set; }
  public int WatchersCount { get; set; }
}

public class IssueAssignableUserDto
{
  public Guid Id { get; set; }
  public string DisplayLabel { get; set; } = string.Empty;
}

public class IssueDetailsDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public IssueStatus Status { get; set; }
  public IssuePriority Priority { get; set; }
  public Guid CreatedBy { get; set; }
  public string? CreatedByName { get; set; }
  public Guid? AssignedToUserId { get; set; }
  public string? AssignedToUserName { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public DateTime LastActivityAt { get; set; }
  public bool IsWatchedByCurrentUser { get; set; }
  public int WatchersCount { get; set; }
  public IReadOnlyCollection<AcceptanceCriterionDto> AcceptanceCriteria { get; set; } = [];
  public IReadOnlyCollection<IssueCommentDto> Comments { get; set; } = [];
  public IReadOnlyCollection<IssueHistoryEntryDto> HistoryEntries { get; set; } = [];
}

public class AcceptanceCriterionDto
{
  public Guid Id { get; set; }
  public string Content { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public Guid CreatedBy { get; set; }
}

public class IssueCommentDto
{
  public Guid Id { get; set; }
  public string Content { get; set; } = string.Empty;
  public Guid AuthorUserId { get; set; }
  public string? AuthorName { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class IssueHistoryEntryDto
{
  public Guid Id { get; set; }
  public IssueHistoryEventType EventType { get; set; }
  public Guid ActorUserId { get; set; }
  public string? ActorName { get; set; }
  public string Summary { get; set; } = string.Empty;
  public string? PreviousValue { get; set; }
  public string? CurrentValue { get; set; }
  public DateTime CreatedAt { get; set; }
}
