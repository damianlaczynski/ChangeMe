using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record GetIssueAttachmentContentQuery(Guid IssueId, Guid AttachmentId) : IQuery<IssueAttachmentContentDto>;

public class GetIssueAttachmentContentHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IFileStorageService fileStorageService) : IQueryHandler<GetIssueAttachmentContentQuery, IssueAttachmentContentDto>
{
  public async ValueTask<Result<IssueAttachmentContentDto>> Handle(
    GetIssueAttachmentContentQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result<IssueAttachmentContentDto>.Unauthorized();

    if (!IssueAuthorization.CanView(userAccessor))
      return Result<IssueAttachmentContentDto>.Forbidden(IssueAuthorization.PermissionDeniedMessage);

    var attachment = await context.IssueAttachments
      .AsNoTracking()
      .FirstOrDefaultAsync(
        a => a.OwnerId == query.IssueId
          && a.Id == query.AttachmentId,
        cancellationToken);

    if (attachment is null)
      return Result<IssueAttachmentContentDto>.NotFound();

    var streamResult = await fileStorageService.OpenReadStreamAsync(
      attachment.StorageContainer,
      attachment.OwnerId,
      attachment.StorageKey,
      cancellationToken);

    if (!streamResult.IsSuccess)
      return streamResult.Map();

    return Result.Success(new IssueAttachmentContentDto
    {
      Content = streamResult.Value,
      OriginalFileName = attachment.OriginalFileName,
      ContentType = attachment.ContentType
    });
  }
}
