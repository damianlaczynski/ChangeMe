using ChangeMe.Backend.Domain.Aggregates.Project.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Project.Entities;

public class ProjectMembershipHistoryEntry : Entity
{
  private ProjectMembershipHistoryEntry() { }

  public Guid ProjectId { get; private set; }
  public ProjectMembershipHistoryEventType EventType { get; private set; }
  public Guid ActorUserId { get; private set; }
  public Guid AffectedUserId { get; private set; }
  public string Summary { get; private set; } = string.Empty;
  public string? PreviousValue { get; private set; }
  public string? CurrentValue { get; private set; }

  public static Result<ProjectMembershipHistoryEntry> Create(
    Guid projectId,
    ProjectMembershipHistoryEventType eventType,
    Guid actorUserId,
    Guid affectedUserId,
    string summary,
    string? previousValue = null,
    string? currentValue = null)
  {
    var validationErrors = new List<ValidationError>();

    if (projectId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(ProjectId), "cannot be empty"));

    if (affectedUserId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(AffectedUserId), "cannot be empty"));

    if (!Enum.IsDefined(eventType))
      validationErrors.Add(new ValidationError(nameof(EventType), "invalid event type"));

    if (string.IsNullOrWhiteSpace(summary))
      validationErrors.Add(new ValidationError(nameof(Summary), "cannot be empty"));
    else if (summary.Trim().Length > ProjectHistoryConstraints.SUMMARY_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Summary), $"cannot be longer than {ProjectHistoryConstraints.SUMMARY_MAX_LENGTH} characters"));

    if (previousValue?.Length > ProjectHistoryConstraints.VALUE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(PreviousValue), $"cannot be longer than {ProjectHistoryConstraints.VALUE_MAX_LENGTH} characters"));

    if (currentValue?.Length > ProjectHistoryConstraints.VALUE_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(CurrentValue), $"cannot be longer than {ProjectHistoryConstraints.VALUE_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new ProjectMembershipHistoryEntry
    {
      ProjectId = projectId,
      EventType = eventType,
      ActorUserId = actorUserId,
      AffectedUserId = affectedUserId,
      Summary = summary.Trim(),
      PreviousValue = previousValue,
      CurrentValue = currentValue,
    });
  }
}
