using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Services;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record AddIssueCommentCommand(Guid IssueId, string Content) : ICommand<IssueDetailsDto>;

public class AddIssueCommentHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IssueNotificationService issueNotificationService) : ICommandHandler<AddIssueCommentCommand, IssueDetailsDto>
{
  public async ValueTask<Result<IssueDetailsDto>> Handle(AddIssueCommentCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var issue = await context.Issues
      .Include(i => i.Comments)
      .Include(i => i.HistoryEntries)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.IssueId, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    if (!IssueAuthorization.CanComment(userAccessor, issue, actorUserId))
      return Result.Forbidden(IssueAuthorization.PermissionDeniedMessage);

    var commentResult = issue.AddComment(command.Content);
    if (!commentResult.IsSuccess)
      return commentResult.Map();

    context.IssueComments.Add(commentResult.Value);

    await context.SaveChangesAsync(cancellationToken);
    await issueNotificationService.NotifyCommentAddedAsync(issue.Id, commentResult.Value.Id, actorUserId, cancellationToken);

    var issueResult = await mediator.Send(new GetIssueByIdQuery(issue.Id), cancellationToken);
    if (!issueResult.IsSuccess)
      return issueResult.Map();

    return issueResult;
  }
}
