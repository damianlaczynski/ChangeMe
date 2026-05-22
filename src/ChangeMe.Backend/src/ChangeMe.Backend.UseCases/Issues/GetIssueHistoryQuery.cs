using ChangeMe.Backend.UseCases.Common;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public sealed class GetIssueHistoryQuery : PaginationQuery<IssueHistoryEntryDto>
{
  public Guid IssueId { get; set; }
}

public class GetIssueHistoryHandler(ApplicationDbContext context)
  : IQueryHandler<GetIssueHistoryQuery, PaginationResult<IssueHistoryEntryDto>>
{
  public async Task<Result<PaginationResult<IssueHistoryEntryDto>>> Handle(
    GetIssueHistoryQuery query,
    CancellationToken cancellationToken)
  {
    var issueExists = await context.Issues.AsNoTracking().AnyAsync(i => i.Id == query.IssueId, cancellationToken);
    if (!issueExists)
      return Result<PaginationResult<IssueHistoryEntryDto>>.NotFound();

    var projected = context.IssueHistoryEntries
      .AsNoTracking()
      .Where(h => h.IssueId == query.IssueId)
      .Select(h => new IssueHistoryEntryDto
      {
        Id = h.Id,
        EventType = h.EventType,
        ActorUserId = h.ActorUserId,
        Summary = h.Summary,
        PreviousValue = h.PreviousValue,
        CurrentValue = h.CurrentValue,
        CreatedAt = h.CreatedAt
      });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      IssueMappingExtensions.CollectHistoryRelatedUserIds(paged.Items),
      cancellationToken);

    var items = paged.Items
      .Select(entry => IssueMappingExtensions.ToHistoryEntryDto(
        entry.EventType,
        entry.ActorUserId,
        entry.Summary,
        entry.PreviousValue,
        entry.CurrentValue,
        entry.CreatedAt,
        entry.Id,
        userLookup))
      .ToList();

    return Result.Success(PaginationResult<IssueHistoryEntryDto>.Create(
      items,
      paged.TotalCount,
      paged.CurrentPage,
      paged.PageSize,
      paged.SortField,
      paged.Ascending));
  }
}
