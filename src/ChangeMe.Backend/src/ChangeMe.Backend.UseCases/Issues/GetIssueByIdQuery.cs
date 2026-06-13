using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record GetIssueByIdQuery(Guid Id) : IQuery<IssueDetailsDto>;

public class GetIssueByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetIssueByIdQuery, IssueDetailsDto>
{
  public async ValueTask<Result<IssueDetailsDto>> Handle(GetIssueByIdQuery query, CancellationToken cancellationToken)
  {
    var issue = await context.Issues
      .AsNoTracking()
      .Include(i => i.AcceptanceCriteria)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var project = await context.Projects
      .AsNoTracking()
      .Where(p => p.Id == issue.ProjectId)
      .Select(p => new { p.Id, p.Key, p.Name })
      .FirstOrDefaultAsync(cancellationToken);

    if (project is null)
      return Result.NotFound();

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      IssuesUtils.CollectRelatedUserIds(issue),
      cancellationToken);

    return Result.Success(issue.ToDetailsDto(
      userLookup,
      userAccessor.UserId,
      project.Id,
      project.Key,
      project.Name));
  }
}
