using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed class GetMySessionsQuery : PaginationQuery<UserSessionDto>
{
  public bool DoNothing { get; set; }
}

public class GetMySessionsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetMySessionsQuery, PaginationResult<UserSessionDto>>
{
  public async Task<Result<PaginationResult<UserSessionDto>>> Handle(
    GetMySessionsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<PaginationResult<UserSessionDto>>.Unauthorized();

    var utcNow = DateTime.UtcNow;
    var currentSessionId = userAccessor.SessionId;

    var sessions = await context.UserSessions
      .AsNoTracking()
      .Where(x => x.UserId == userId && x.RevokedAt == null)
      .ToListAsync(cancellationToken);

    var activeSessions = sessions
      .Where(x => sessionLifetime.IsActive(x, utcNow))
      .Select(x => new UserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.IpAddress,
        x.SignedInAt,
        x.LastActivityAt,
        currentSessionId.HasValue && x.Id == currentSessionId.Value))
      .AsQueryable();

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedSessions = await activeSessions.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedSessions);
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "SignedInAt" => nameof(UserSessionDto.SignedInAt),
      "DeviceBrowserLabel" => nameof(UserSessionDto.DeviceBrowserLabel),
      "LastActivityAt" or _ => nameof(UserSessionDto.LastActivityAt)
    };
}
