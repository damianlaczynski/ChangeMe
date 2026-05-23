using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Issue;

public class Issue : Entity, IAggregateRoot
{
  private readonly List<IssueAcceptanceCriterion> acceptanceCriteria = new();
  private readonly List<IssueComment> comments = new();
  private readonly List<IssueHistoryEntry> historyEntries = new();
  private readonly List<IssueWatcher> watchers = new();

  private Issue() { }

  public string Title { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public IssueStatus Status { get; private set; } = IssueStatus.NEW;
  public IssuePriority Priority { get; private set; } = IssuePriority.MEDIUM;
  public Guid? AssignedToUserId { get; private set; }
  public DateTime LastActivityAt { get; private set; }

  public IReadOnlyCollection<IssueAcceptanceCriterion> AcceptanceCriteria => acceptanceCriteria.AsReadOnly();
  public IReadOnlyCollection<IssueComment> Comments => comments.AsReadOnly();
  public IReadOnlyCollection<IssueHistoryEntry> HistoryEntries => historyEntries.AsReadOnly();
  public IReadOnlyCollection<IssueWatcher> Watchers => watchers.AsReadOnly();

  public static Result<Issue> Create(
    string title,
    string description,
    IssuePriority priority = IssuePriority.MEDIUM,
    IssueStatus status = IssueStatus.NEW,
    Guid? assignedToUserId = null)
  {
    var validationErrors = Validate(title, description, priority, status);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var issue = new Issue
    {
      Title = title.Trim(),
      Description = description.Trim(),
      Priority = priority,
      Status = status,
      AssignedToUserId = assignedToUserId,
      LastActivityAt = DateTime.UtcNow,
    };

    return Result.Success(issue);
  }

  public Result<Issue> RecordCreation(Guid actorUserId)
  {
    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ISSUE_CREATED,
      actorUserId,
      $"Issue '{Title}' was created.");

    if (!historyResult.IsSuccess)
      return historyResult.Map();

    return Result.Success(this);
  }

  public Result<Issue> UpdateDetails(
    string title,
    string description,
    IssuePriority priority,
    IssueStatus status,
    Guid? assignedToUserId,
    Guid actorUserId)
  {
    var validationErrors = Validate(title, description, priority, status);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var normalizedTitle = title.Trim();
    var normalizedDescription = description.Trim();
    var hadChanges = false;

    if (!string.Equals(Title, normalizedTitle, StringComparison.Ordinal))
    {
      var historyResult = AddHistoryEntry(
        IssueHistoryEventType.TITLE_CHANGED,
        actorUserId,
        "Issue title changed.",
        Title,
        normalizedTitle);
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      Title = normalizedTitle;
      hadChanges = true;
    }

    if (!string.Equals(Description, normalizedDescription, StringComparison.Ordinal))
    {
      var historyResult = AddHistoryEntry(
        IssueHistoryEventType.DESCRIPTION_CHANGED,
        actorUserId,
        "Issue description changed.",
        Description,
        normalizedDescription);
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      Description = normalizedDescription;
      hadChanges = true;
    }

    if (Priority != priority)
    {
      var historyResult = AddHistoryEntry(
        IssueHistoryEventType.PRIORITY_CHANGED,
        actorUserId,
        "Issue priority changed.",
        Priority.ToString(),
        priority.ToString());
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      Priority = priority;
      hadChanges = true;
    }

    if (Status != status)
    {
      var eventType = IsStatusCloseOrReopen(Status, status)
        ? IssueHistoryEventType.STATUS_CHANGED
        : IssueHistoryEventType.STATUS_CHANGED;

      var historyResult = AddHistoryEntry(
        eventType,
        actorUserId,
        "Issue status changed.",
        Status.ToString(),
        status.ToString());
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      Status = status;
      hadChanges = true;
    }

    if (AssignedToUserId != assignedToUserId)
    {
      var historyResult = AddHistoryEntry(
        IssueHistoryEventType.ASSIGNEE_CHANGED,
        actorUserId,
        "Issue assignee changed.",
        AssignedToUserId?.ToString(),
        assignedToUserId?.ToString());
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      AssignedToUserId = assignedToUserId;
      hadChanges = true;
    }

    if (hadChanges)
      LastActivityAt = DateTime.UtcNow;

    return Result.Success(this);
  }

  public Result<IssueAcceptanceCriterion> AddAcceptanceCriterion(string content)
  {
    var acceptanceCriterion = IssueAcceptanceCriterion.Create(Id, content);
    if (!acceptanceCriterion.IsSuccess)
      return acceptanceCriterion.Map();

    acceptanceCriteria.Add(acceptanceCriterion.Value);
    return Result.Success(acceptanceCriterion.Value);
  }

  public Result<IssueAcceptanceCriterion> AddAcceptanceCriterion(string content, Guid actorUserId)
  {
    var acceptanceCriterionResult = AddAcceptanceCriterion(content);
    if (!acceptanceCriterionResult.IsSuccess)
      return acceptanceCriterionResult.Map();

    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED,
      actorUserId,
      "Acceptance criterion added.",
      null,
      acceptanceCriterionResult.Value.Content);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    LastActivityAt = DateTime.UtcNow;
    return acceptanceCriterionResult;
  }

  public Result<IssueAcceptanceCriterion> UpdateAcceptanceCriterion(Guid acceptanceCriterionId, string content)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    return acceptanceCriterion.UpdateContent(content);
  }

  public Result<IssueAcceptanceCriterion> UpdateAcceptanceCriterion(Guid acceptanceCriterionId, string content, Guid actorUserId)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    var previousContent = acceptanceCriterion.Content;
    var updateResult = acceptanceCriterion.UpdateContent(content);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    if (!string.Equals(previousContent, updateResult.Value.Content, StringComparison.Ordinal))
    {
      var historyResult = AddHistoryEntry(
        IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED,
        actorUserId,
        "Acceptance criterion updated.",
        previousContent,
        updateResult.Value.Content);
      if (!historyResult.IsSuccess)
        return historyResult.Map();

      LastActivityAt = DateTime.UtcNow;
    }

    return updateResult;
  }

  public Result<IssueAcceptanceCriterion> RemoveAcceptanceCriterion(Guid acceptanceCriterionId)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    acceptanceCriteria.Remove(acceptanceCriterion);
    return Result.Success(acceptanceCriterion);
  }

  public Result<IssueAcceptanceCriterion> RemoveAcceptanceCriterion(Guid acceptanceCriterionId, Guid actorUserId)
  {
    var acceptanceCriterion = acceptanceCriteria.FirstOrDefault(c => c.Id == acceptanceCriterionId);
    if (acceptanceCriterion is null)
      return Result.NotFound();

    var historyResult = AddHistoryEntry(
      IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED,
      actorUserId,
      "Acceptance criterion removed.",
      acceptanceCriterion.Content,
      null);
    if (!historyResult.IsSuccess)
      return historyResult.Map();

    acceptanceCriteria.Remove(acceptanceCriterion);
    LastActivityAt = DateTime.UtcNow;
    return Result.Success(acceptanceCriterion);
  }

  public Result<IssueComment> AddComment(string content)
  {
    var commentResult = IssueComment.Create(Id, content);
    if (!commentResult.IsSuccess)
      return commentResult.Map();

    comments.Add(commentResult.Value);
    LastActivityAt = DateTime.UtcNow;

    return Result.Success(commentResult.Value);
  }

  public Result<IssueWatcher> StartWatching(Guid userId)
  {
    if (watchers.Any(w => w.UserId == userId))
      return Result.Conflict("Issue is already watched by this user.");

    var watcher = IssueWatcher.Create(Id, userId);
    if (!watcher.IsSuccess)
      return watcher.Map();

    watchers.Add(watcher.Value);
    return Result.Success(watcher.Value);
  }

  public Result<IssueWatcher> StopWatching(Guid userId)
  {
    var watcher = watchers.FirstOrDefault(w => w.UserId == userId);
    if (watcher is null)
      return Result.NotFound();

    watchers.Remove(watcher);
    return Result.Success(watcher);
  }

  public bool IsWatchedBy(Guid userId) => watchers.Any(w => w.UserId == userId);

  private Result<IssueHistoryEntry> AddHistoryEntry(
    IssueHistoryEventType eventType,
    Guid actorUserId,
    string summary,
    string? previousValue = null,
    string? currentValue = null,
    Guid? relatedCommentId = null)
  {
    var historyEntryResult = IssueHistoryEntry.Create(
      Id,
      eventType,
      actorUserId,
      summary,
      previousValue,
      currentValue,
      relatedCommentId);

    if (!historyEntryResult.IsSuccess)
      return historyEntryResult.Map();

    historyEntries.Add(historyEntryResult.Value);
    return Result.Success(historyEntryResult.Value);
  }

  private static List<ValidationError> Validate(
    string title,
    string description,
    IssuePriority priority,
    IssueStatus status)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(title))
      validationErrors.Add(new ValidationError(nameof(Title), "cannot be empty"));
    else
    {
      if (title.Trim().Length < IssueConstraints.TITLE_MIN_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Title), $"must be at least {IssueConstraints.TITLE_MIN_LENGTH} characters long"));
      if (title.Trim().Length > IssueConstraints.TITLE_MAX_LENGTH)
        validationErrors.Add(new ValidationError(nameof(Title), $"cannot be longer than {IssueConstraints.TITLE_MAX_LENGTH} characters"));
    }

    if (string.IsNullOrWhiteSpace(description))
      validationErrors.Add(new ValidationError(nameof(Description), "cannot be empty"));
    else if (description.Trim().Length > IssueConstraints.DESCRIPTION_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {IssueConstraints.DESCRIPTION_MAX_LENGTH} characters"));

    if (!Enum.IsDefined(priority))
      validationErrors.Add(new ValidationError(nameof(Priority), "invalid priority"));

    if (!Enum.IsDefined(status))
      validationErrors.Add(new ValidationError(nameof(Status), "invalid status"));

    return validationErrors;
  }

  private static bool IsStatusCloseOrReopen(IssueStatus previousStatus, IssueStatus nextStatus)
  {
    return (previousStatus == IssueStatus.CLOSED && nextStatus != IssueStatus.CLOSED)
      || (previousStatus != IssueStatus.CLOSED && nextStatus == IssueStatus.CLOSED);
  }
}

public static class IssueConstraints
{
  public const int TITLE_MIN_LENGTH = 3;
  public const int TITLE_MAX_LENGTH = 255;
  public const int DESCRIPTION_MAX_LENGTH = 2000;
}
