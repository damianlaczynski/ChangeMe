using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetMySessionsQuery(bool doNothing = false) : IQuery<IReadOnlyList<UserSessionDto>>;

public class GetMySessionsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetMySessionsQuery, IReadOnlyList<UserSessionDto>>
{
  public async Task<Result<IReadOnlyList<UserSessionDto>>> Handle(
    GetMySessionsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<IReadOnlyList<UserSessionDto>>.Unauthorized();

    var utcNow = DateTime.UtcNow;
    var currentSessionId = userAccessor.SessionId;

    var sessions = await context.UserSessions
      .AsNoTracking()
      .Where(x => x.UserId == userId && x.RevokedAt == null)
      .OrderByDescending(x => x.LastActivityAt)
      .ToListAsync(cancellationToken);

    var activeSessions = sessions
      .Where(x => sessionLifetime.IsActive(x, utcNow))
      .Select(x => new UserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.IpAddress,
        x.IsPersistent,
        x.SignedInAt,
        x.LastActivityAt,
        currentSessionId.HasValue && x.Id == currentSessionId.Value))
      .ToList();

    return Result.Success<IReadOnlyList<UserSessionDto>>(activeSessions);
  }
}
