using ChangeMe.Backend.Domain.Aggregates.Project.Enums;

namespace ChangeMe.Backend.UseCases.Projects.Dtos;

public class ProjectListItemDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public int IssueCount { get; set; }
  public ProjectRole CurrentUserRole { get; set; }
  public bool IsSystem { get; set; }
  public bool CanManage { get; set; }
}

public class ProjectDetailsDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public bool IsSystem { get; set; }
  public ProjectRole CurrentUserRole { get; set; }
  public int IssueCount { get; set; }
  public bool CanManage { get; set; }
  public bool CanViewMembers { get; set; }
  public bool CanManageMembers { get; set; }
  public bool CanViewIssues { get; set; }
  public bool CanManageIssues { get; set; }
  public bool CanViewLoggedTime { get; set; }
  public int LoggedTimeMinutes { get; set; }
  public string LoggedTimeFormatted { get; set; } = string.Empty;
}

public class ProjectOptionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
}

public class ProjectMemberDto
{
  public Guid UserId { get; set; }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string Email { get; set; } = string.Empty;
  public ProjectRole Role { get; set; }
  public bool Deactivated { get; set; }
  public bool CanViewUserDetails { get; set; }
}

public class ProjectMembershipHistoryEntryDto
{
  public Guid Id { get; set; }
  public ProjectMembershipHistoryEventType EventType { get; set; }
  public Guid ActorUserId { get; set; }
  public string? ActorName { get; set; }
  public Guid AffectedUserId { get; set; }
  public string? AffectedUserName { get; set; }
  public string Summary { get; set; } = string.Empty;
  public string? PreviousValue { get; set; }
  public string? CurrentValue { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class ProjectOperationHistoryEntryDto
{
  public Guid Id { get; set; }
  public ProjectOperationHistoryEventType EventType { get; set; }
  public Guid ActorUserId { get; set; }
  public string? ActorName { get; set; }
  public string Summary { get; set; } = string.Empty;
  public string? PreviousValue { get; set; }
  public string? CurrentValue { get; set; }
  public DateTime CreatedAt { get; set; }
}
