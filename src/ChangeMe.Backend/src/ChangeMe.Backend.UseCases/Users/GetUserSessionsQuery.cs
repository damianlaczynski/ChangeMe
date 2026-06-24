using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed class GetUserSessionsQuery : PaginationQuery<AdminUserSessionDto>
{
  public Guid Id { get; set; }
}

public class GetUserSessionsHandler(
  ApplicationDbContext context,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetUserSessionsQuery, PaginationResult<AdminUserSessionDto>>
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
    var sessionLifetimeDays = sessionLifetime.SessionLifetimeDays;

    var activeSessionsQuery = context.UserSessions
      .AsNoTracking()
      .WhereActiveSessions(query.Id, utcNow, sessionLifetimeDays)
      .Select(x => new AdminUserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.SignInMethod,
        x.IpAddress,
        x.SignedInAt,
        x.LastActivityAt));

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedSessions = await activeSessionsQuery.ToPaginationResultAsync(
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
