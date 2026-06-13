using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;

namespace ChangeMe.Backend.UseCases.Projects.Dtos;

public sealed record ProjectListItemDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Key { get; init; } = string.Empty;
  public string? Description { get; init; }
  public ProjectStatus Status { get; init; }
  public ProjectVisibility Visibility { get; init; }
  public string Color { get; init; } = string.Empty;
  public int IssueCount { get; init; }
  public int MemberCount { get; init; }
  public ProjectMemberRole? CurrentUserRole { get; init; }
}

public sealed record ProjectSelectionItemDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Key { get; init; } = string.Empty;
  public string Color { get; init; } = string.Empty;
  public ProjectStatus Status { get; init; }
}

public sealed record ProjectMemberDto
{
  public Guid UserId { get; init; }
  public string DisplayLabel { get; init; } = string.Empty;
  public ProjectMemberRole Role { get; init; }
  public DateTime JoinedAt { get; init; }
}

public sealed record ProjectDetailsDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Key { get; init; } = string.Empty;
  public string? Description { get; init; }
  public ProjectStatus Status { get; init; }
  public ProjectVisibility Visibility { get; init; }
  public string Color { get; init; } = string.Empty;
  public int IssueCount { get; init; }
  public int MemberCount { get; init; }
  public DateTime CreatedAt { get; init; }
  public DateTime? UpdatedAt { get; init; }
  public IReadOnlyList<ProjectMemberDto> Members { get; init; } = [];
  public ProjectMemberRole? CurrentUserRole { get; init; }
}

public sealed record ProjectOverviewDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string Key { get; init; } = string.Empty;
  public string? Description { get; init; }
  public ProjectStatus Status { get; init; }
  public ProjectVisibility Visibility { get; init; }
  public string Color { get; init; } = string.Empty;
  public int TotalIssues { get; init; }
  public int NewIssues { get; init; }
  public int InProgressIssues { get; init; }
  public int ResolvedIssues { get; init; }
  public int ClosedIssues { get; init; }
  public int MemberCount { get; init; }
}
