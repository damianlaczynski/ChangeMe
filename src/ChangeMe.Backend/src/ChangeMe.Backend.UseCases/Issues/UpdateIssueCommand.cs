using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Services;

namespace ChangeMe.Backend.UseCases.Issues;

public record UpdateIssueCommand(
  Guid Id,
  string Title,
  string Description,
  IssueStatus Status,
  IssuePriority Priority,
  Guid? AssignedToUserId,
  List<UpdateIssueAcceptanceCriterionPayload>? AcceptanceCriteria = null) : ICommand<IssueDetailsDto>;

public record UpdateIssueAcceptanceCriterionPayload(Guid? Id, string Content);

public class UpdateIssueHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IssueNotificationService issueNotificationService,
  IIssueRealtimePublisher issueRealtimePublisher) : ICommandHandler<UpdateIssueCommand, IssueDetailsDto>
{
  public async Task<Result<IssueDetailsDto>> Handle(UpdateIssueCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var issue = await context.Issues
      .Include(i => i.AcceptanceCriteria)
      .Include(i => i.HistoryEntries)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    if (command.AssignedToUserId.HasValue)
    {
      var assigneeExists = await context.Users
        .AsNoTracking()
        .AnyAsync(u => u.Id == command.AssignedToUserId.Value, cancellationToken);

      if (!assigneeExists)
        return Result.Invalid([
          new ValidationError(nameof(command.AssignedToUserId), "assigned user does not exist")
        ]);
    }

    var updateResult = issue.UpdateDetails(
      command.Title,
      command.Description,
      command.Priority,
      command.Status,
      command.AssignedToUserId,
      actorUserId);

    if (!updateResult.IsSuccess)
      return updateResult.Map();

    var acceptanceCriteriaResult = UpdateAcceptanceCriteria(command.AcceptanceCriteria, issue, actorUserId);
    if (!acceptanceCriteriaResult.IsSuccess)
      return acceptanceCriteriaResult.Map();

    var newHistoryEntries = issue.HistoryEntries
      .Where(h => h.CreatedAt == default)
      .ToList();

    if (newHistoryEntries.Count > 0)
      await context.IssueHistoryEntries.AddRangeAsync(newHistoryEntries, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    foreach (var historyEntryId in newHistoryEntries
               .Where(h => IsNotificationEligible(h.EventType))
               .Select(h => h.Id))
      await issueNotificationService.NotifyIssueActivityAsync(issue.Id, historyEntryId, actorUserId, cancellationToken);

    await issueRealtimePublisher.PublishAsync(new IssueRealtimeMessage
    {
      IssueId = issue.Id,
      EventType = "ISSUE_UPDATED",
      OccurredAt = issue.LastActivityAt
    }, cancellationToken);

    var updatedIssueResult = await mediator.Send(new GetIssueByIdQuery(issue.Id), cancellationToken);
    if (!updatedIssueResult.IsSuccess)
      return updatedIssueResult.Map();

    return Result.Success(updatedIssueResult.Value);
  }

  private Result UpdateAcceptanceCriteria(List<UpdateIssueAcceptanceCriterionPayload>? acceptanceCriteria, Issue issue, Guid actorUserId)
  {
    if (acceptanceCriteria is null)
      return Result.Success();

    var retainedAcceptanceCriteriaIds = new HashSet<Guid>();

    foreach (var acceptanceCriterion in acceptanceCriteria)
    {
      if (acceptanceCriterion.Id.HasValue)
      {
        var updateAcceptanceCriterionResult = issue.UpdateAcceptanceCriterion(
          acceptanceCriterion.Id.Value,
          acceptanceCriterion.Content,
          actorUserId);
        if (!updateAcceptanceCriterionResult.IsSuccess)
          return updateAcceptanceCriterionResult.Map();

        retainedAcceptanceCriteriaIds.Add(acceptanceCriterion.Id.Value);
      }
      else
      {
        var addAcceptanceCriterionResult = issue.AddAcceptanceCriterion(acceptanceCriterion.Content, actorUserId);
        if (!addAcceptanceCriterionResult.IsSuccess)
          return addAcceptanceCriterionResult.Map();

        context.IssueAcceptanceCriteria.Add(addAcceptanceCriterionResult.Value);
        retainedAcceptanceCriteriaIds.Add(addAcceptanceCriterionResult.Value.Id);
      }
    }

    var acceptanceCriteriaToRemove = issue.AcceptanceCriteria
      .Where(criterion => !retainedAcceptanceCriteriaIds.Contains(criterion.Id))
      .ToList();

    foreach (var acceptanceCriterion in acceptanceCriteriaToRemove)
    {
      var removeAcceptanceCriterionResult = issue.RemoveAcceptanceCriterion(acceptanceCriterion.Id, actorUserId);
      if (!removeAcceptanceCriterionResult.IsSuccess)
        return removeAcceptanceCriterionResult.Map();

      context.IssueAcceptanceCriteria.Remove(removeAcceptanceCriterionResult.Value);
    }

    return Result.Success();
  }

  private static bool IsNotificationEligible(IssueHistoryEventType eventType)
  {
    return eventType is
      IssueHistoryEventType.STATUS_CHANGED or
      IssueHistoryEventType.PRIORITY_CHANGED or
      IssueHistoryEventType.ASSIGNEE_CHANGED or
      IssueHistoryEventType.TITLE_CHANGED or
      IssueHistoryEventType.DESCRIPTION_CHANGED or
      IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED or
      IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED or
      IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED;
  }
}
