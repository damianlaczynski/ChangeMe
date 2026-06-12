using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public sealed class GetIssueAttachmentsQuery : PaginationQuery<IssueAttachmentDto>
{
  public Guid IssueId { get; set; }
}

public class GetIssueAttachmentsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetIssueAttachmentsQuery, PaginationResult<IssueAttachmentDto>>
{
  public async ValueTask<Result<PaginationResult<IssueAttachmentDto>>> Handle(
    GetIssueAttachmentsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result<PaginationResult<IssueAttachmentDto>>.Unauthorized();

    var issueExists = await context.Issues.AsNoTracking().AnyAsync(i => i.Id == query.IssueId, cancellationToken);
    if (!issueExists)
      return Result<PaginationResult<IssueAttachmentDto>>.NotFound();

    var projected = context.IssueAttachments
      .AsNoTracking()
      .Where(a => a.OwnerId == query.IssueId)
      .Select(a => new IssueAttachmentDto
      {
        Id = a.Id,
        OriginalFileName = a.OriginalFileName,
        ContentType = a.ContentType,
        SizeBytes = a.SizeBytes,
        UploadedByUserId = a.CreatedBy,
        CreatedAt = a.CreatedAt,
        CanDelete = a.CreatedBy == currentUserId
      });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      paged.Items.Select(a => a.UploadedByUserId),
      cancellationToken);

    foreach (var item in paged.Items)
      item.UploadedByName = userLookup.GetValueOrDefault(item.UploadedByUserId);

    return Result.Success(paged);
  }
}
