using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.UseCases.Issues.Utils;

public enum IssueEditField
{
  Title,
  Description,
  Status,
  Priority,
  AssignedTo,
  AcceptanceCriteria
}

public static class IssueAuthorization
{
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";

  public static bool CanView(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.IssuesView);

  public static bool CanCreate(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.IssuesCreate);

  public static bool CanDelete(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.IssuesDelete);

  public static bool CanManageAttachments(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.IssuesManageAttachments);

  public static bool CanAssignUsers(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.IssuesCreate)
    || userAccessor.HasPermission(PermissionCodes.IssuesEdit);

  public static bool CanComment(IUserAccessor userAccessor, Issue issue, Guid userId) =>
    userAccessor.HasPermission(PermissionCodes.IssuesComment)
    || (userAccessor.HasPermission(PermissionCodes.IssuesView) && IsAuthor(issue, userId));

  public static bool CanDeleteAttachment(IUserAccessor userAccessor, Guid uploadedByUserId, Guid actorUserId) =>
    userAccessor.HasPermission(PermissionCodes.IssuesManageAttachments)
    && uploadedByUserId == actorUserId;

  public static bool CanEditField(IUserAccessor userAccessor, Issue issue, Guid userId, IssueEditField field)
  {
    if (userAccessor.HasPermission(PermissionCodes.IssuesEdit))
      return true;

    if (!userAccessor.HasPermission(PermissionCodes.IssuesView))
      return false;

    var isAuthor = IsAuthor(issue, userId);
    var isAssignee = IsAssignee(issue, userId);

    return field switch
    {
      IssueEditField.Title or IssueEditField.Description or IssueEditField.AcceptanceCriteria => isAuthor,
      IssueEditField.Status or IssueEditField.Priority => isAuthor || isAssignee,
      IssueEditField.AssignedTo => false,
      _ => false
    };
  }

  public static bool HasAcceptanceCriteriaChanges(
    Issue issue,
    IReadOnlyList<UpdateIssueAcceptanceCriterionPayload>? acceptanceCriteria)
  {
    if (acceptanceCriteria is null)
      return false;

    var retainedIds = acceptanceCriteria
      .Where(x => x.Id.HasValue)
      .Select(x => x.Id!.Value)
      .ToHashSet();

    if (issue.AcceptanceCriteria.Any(criterion => !retainedIds.Contains(criterion.Id)))
      return true;

    if (acceptanceCriteria.Any(x => !x.Id.HasValue))
      return true;

    foreach (var payload in acceptanceCriteria)
    {
      if (!payload.Id.HasValue)
        continue;

      var existing = issue.AcceptanceCriteria.FirstOrDefault(x => x.Id == payload.Id.Value);
      if (existing is null)
        return true;

      if (!string.Equals(existing.Content.Trim(), payload.Content.Trim(), StringComparison.Ordinal))
        return true;
    }

    return false;
  }

  public static Result ValidateUpdatePermissions(
    IUserAccessor userAccessor,
    Issue issue,
    Guid userId,
    UpdateIssueCommand command)
  {
    if (!string.Equals(issue.Title, command.Title.Trim(), StringComparison.Ordinal)
        && !CanEditField(userAccessor, issue, userId, IssueEditField.Title))
      return Result.Forbidden(PermissionDeniedMessage);

    if (!string.Equals(issue.Description, command.Description.Trim(), StringComparison.Ordinal)
        && !CanEditField(userAccessor, issue, userId, IssueEditField.Description))
      return Result.Forbidden(PermissionDeniedMessage);

    if (issue.Status != command.Status
        && !CanEditField(userAccessor, issue, userId, IssueEditField.Status))
      return Result.Forbidden(PermissionDeniedMessage);

    if (issue.Priority != command.Priority
        && !CanEditField(userAccessor, issue, userId, IssueEditField.Priority))
      return Result.Forbidden(PermissionDeniedMessage);

    if (issue.AssignedToUserId != command.AssignedToUserId
        && !CanEditField(userAccessor, issue, userId, IssueEditField.AssignedTo))
      return Result.Forbidden(PermissionDeniedMessage);

    if (HasAcceptanceCriteriaChanges(issue, command.AcceptanceCriteria)
        && !CanEditField(userAccessor, issue, userId, IssueEditField.AcceptanceCriteria))
      return Result.Forbidden(PermissionDeniedMessage);

    return Result.Success();
  }

  private static bool IsAuthor(Issue issue, Guid userId) => issue.CreatedBy == userId;

  private static bool IsAssignee(Issue issue, Guid userId) => issue.AssignedToUserId == userId;
}
