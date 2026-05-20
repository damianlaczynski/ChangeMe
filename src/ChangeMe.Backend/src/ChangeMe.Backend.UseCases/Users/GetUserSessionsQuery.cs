using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record GetUserSessionsQuery(Guid Id) : IQuery<IReadOnlyList<AdminUserSessionDto>>;

public class GetUserSessionsHandler(
  ApplicationDbContext context,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetUserSessionsQuery, IReadOnlyList<AdminUserSessionDto>>
{
  public async Task<Result<IReadOnlyList<AdminUserSessionDto>>> Handle(
    GetUserSessionsQuery query,
    CancellationToken cancellationToken)
  {
    var userExists = await context.Users.AnyAsync(x => x.Id == query.Id, cancellationToken);
    if (!userExists)
      return Result<IReadOnlyList<AdminUserSessionDto>>.NotFound();

    var user = await context.Users.AsNoTracking().FirstAsync(x => x.Id == query.Id, cancellationToken);
    if (!user.IsActive)
      return Result.Success<IReadOnlyList<AdminUserSessionDto>>([]);

    var utcNow = DateTime.UtcNow;
    var sessions = await context.UserSessions
      .AsNoTracking()
      .Where(x => x.UserId == query.Id && x.RevokedAt == null)
      .ToListAsync(cancellationToken);

    return Result.Success(UsersSupport.MapActiveSessions(sessions, utcNow, sessionLifetime));
  }
}
