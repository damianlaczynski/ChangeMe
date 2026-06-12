using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Users;

public sealed class GetUserSessionsQuery : PaginationQuery<AdminUserSessionDto>
{
  public Guid Id { get; set; }
}

public class GetUserSessionsHandler(
  ApplicationDbContext context,
  ISessionLifetimeService sessionLifetime,
  IOptions<AuthOptions> authOptions) : IQueryHandler<GetUserSessionsQuery, PaginationResult<AdminUserSessionDto>>
{
  public async ValueTask<Result<PaginationResult<AdminUserSessionDto>>> Handle(
    GetUserSessionsQuery query,
    CancellationToken cancellationToken)
  {
    var userExists = await context.Users.AnyAsync(x => x.Id == query.Id, cancellationToken);
    if (!userExists)
      return Result<PaginationResult<AdminUserSessionDto>>.NotFound();

    var user = await context.Users.AsNoTracking().FirstAsync(x => x.Id == query.Id, cancellationToken);
    if (!user.IsActive)
      return Result.Success(PaginationResult<AdminUserSessionDto>.Empty());

    var utcNow = DateTime.UtcNow;
    var sessions = await context.UserSessions
      .AsNoTracking()
      .Where(x => x.UserId == query.Id && x.RevokedAt == null)
      .ToListAsync(cancellationToken);

    var activeSessions = UsersUtils.MapActiveSessions(sessions, utcNow, sessionLifetime, authOptions.Value).AsQueryable();

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
      "SignedInAt" => nameof(AdminUserSessionDto.SignedInAt),
      "DeviceBrowserLabel" => nameof(AdminUserSessionDto.DeviceBrowserLabel),
      "LastActivityAt" or _ => nameof(AdminUserSessionDto.LastActivityAt)
    };
}
