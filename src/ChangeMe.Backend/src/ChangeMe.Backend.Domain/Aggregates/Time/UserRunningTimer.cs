namespace ChangeMe.Backend.Domain.Aggregates.Time;

public class UserRunningTimer : Entity, IAggregateRoot
{
  private UserRunningTimer() { }

  public Guid UserId { get; private set; }
  public Guid? ProjectId { get; private set; }
  public Guid? IssueId { get; private set; }
  public DateTime StartedAtUtc { get; private set; }

  public static UserRunningTimer Start(Guid userId, Guid? projectId, Guid? issueId, DateTime startedAtUtc) =>
    new()
    {
      UserId = userId,
      ProjectId = projectId,
      IssueId = issueId,
      StartedAtUtc = startedAtUtc,
    };

  public void Restart(Guid? projectId, Guid? issueId, DateTime startedAtUtc)
  {
    ProjectId = projectId;
    IssueId = issueId;
    StartedAtUtc = startedAtUtc;
  }

  public int GetElapsedWholeMinutes(DateTime utcNow) =>
    Math.Max(0, (int)Math.Floor((utcNow - StartedAtUtc).TotalMinutes));
}
