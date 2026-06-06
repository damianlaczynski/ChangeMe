namespace ChangeMe.Backend.Domain.Aggregates.Time;

public class TimeEntry : Entity, IAggregateRoot
{
  private TimeEntry() { }

  public Guid AuthorUserId { get; private set; }
  public Guid ProjectId { get; private set; }
  public Guid? IssueId { get; private set; }
  public DateOnly WorkDate { get; private set; }
  public int DurationMinutes { get; private set; }
  public string Description { get; private set; } = string.Empty;

  public static Result<TimeEntry> Create(
    Guid authorUserId,
    Guid projectId,
    Guid? issueId,
    DateOnly workDate,
    int durationMinutes,
    string? description)
  {
    var validationErrors = Validate(projectId, durationMinutes, description);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var entry = new TimeEntry
    {
      AuthorUserId = authorUserId,
      ProjectId = projectId,
      IssueId = issueId,
      WorkDate = workDate,
      DurationMinutes = durationMinutes,
      Description = description?.Trim() ?? string.Empty,
    };

    return Result.Success(entry);
  }

  public Result<TimeEntry> Update(
    Guid projectId,
    Guid? issueId,
    DateOnly workDate,
    int durationMinutes,
    string? description)
  {
    var validationErrors = Validate(projectId, durationMinutes, description);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    ProjectId = projectId;
    IssueId = issueId;
    WorkDate = workDate;
    DurationMinutes = durationMinutes;
    Description = description?.Trim() ?? string.Empty;

    return Result.Success(this);
  }

  private static List<ValidationError> Validate(
    Guid projectId,
    int durationMinutes,
    string? description)
  {
    var validationErrors = new List<ValidationError>();

    if (projectId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(ProjectId), TimeConstraints.ProjectRequiredMessage));

    if (durationMinutes < TimeConstraints.MinDurationMinutes
        || durationMinutes > TimeConstraints.MaxDurationMinutes)
      validationErrors.Add(new ValidationError(nameof(DurationMinutes), TimeConstraints.DurationRangeMessage));

    var trimmedDescription = description?.Trim() ?? string.Empty;
    if (trimmedDescription.Length > TimeConstraints.DescriptionMaxLength)
      validationErrors.Add(new ValidationError(nameof(Description), TimeConstraints.DescriptionTooLongMessage));

    return validationErrors;
  }
}
