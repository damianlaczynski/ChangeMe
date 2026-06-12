using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public sealed class GetIssueCommentsQuery : PaginationQuery<IssueCommentDto>
{
  public Guid IssueId { get; set; }
}

public class GetIssueCommentsHandler(ApplicationDbContext context)
  : IQueryHandler<GetIssueCommentsQuery, PaginationResult<IssueCommentDto>>
{
  public async ValueTask<Result<PaginationResult<IssueCommentDto>>> Handle(
    GetIssueCommentsQuery query,
    CancellationToken cancellationToken)
  {
    var issueExists = await context.Issues.AsNoTracking().AnyAsync(i => i.Id == query.IssueId, cancellationToken);
    if (!issueExists)
      return Result<PaginationResult<IssueCommentDto>>.NotFound();

    var projected = context.IssueComments
      .AsNoTracking()
      .Where(c => c.IssueId == query.IssueId)
      .Select(c => new IssueCommentDto
      {
        Id = c.Id,
        Content = c.Content,
        AuthorUserId = c.CreatedBy,
        CreatedAt = c.CreatedAt
      });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      paged.Items.Select(c => c.AuthorUserId),
      cancellationToken);

    foreach (var item in paged.Items)
      item.AuthorName = userLookup.GetValueOrDefault(item.AuthorUserId);

    return Result.Success(paged);
  }
}
