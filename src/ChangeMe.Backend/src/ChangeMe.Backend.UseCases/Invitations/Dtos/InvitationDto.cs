using ChangeMe.Backend.Domain.Aggregates.Invitations.Enums;
using QueryGrid.Abstractions;

namespace ChangeMe.Backend.UseCases.Invitations.Dtos;

public class InvitationListItemDto
{
  [GridSearchable]
  public Guid Id { get; set; }

  [GridSearchable]
  public string Email { get; set; } = string.Empty;

  [GridSearchable]
  public InvitationStatus Status { get; set; }

  public IReadOnlyList<string> RoleNames { get; set; } = [];

  public Guid InvitedByUserId { get; set; }
  public string? InvitedByName { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime ExpiresAt { get; set; }
  public DateTime? AcceptedAt { get; set; }
}

public class InvitationAcceptanceDetailsDto
{
  public string Email { get; set; } = string.Empty;
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
}

public class CreateInvitationResultDto
{
  public Guid Id { get; set; }
}
