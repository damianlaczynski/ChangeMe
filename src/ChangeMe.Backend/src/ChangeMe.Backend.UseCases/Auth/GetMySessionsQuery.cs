using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed class GetMySessionsQuery : PaginationQuery<UserSessionDto> { }

public class GetMySessionsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  ISessionLifetimeService sessionLifetime) : IQueryHandler<GetMySessionsQuery, PaginationResult<UserSessionDto>>
{
  public async ValueTask<Result<PaginationResult<UserSessionDto>>> Handle(
    GetMySessionsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<PaginationResult<UserSessionDto>>.Unauthorized();

    var utcNow = DateTime.UtcNow;
    var currentSessionId = userAccessor.SessionId;
    var sessionLifetimeDays = sessionLifetime.SessionLifetimeDays;

    var activeSessionsQuery = context.UserSessions
      .AsNoTracking()
      .WhereActiveSessions(userId, utcNow, sessionLifetimeDays);

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedSessions = await activeSessionsQuery.ToPaginationResultAsync(
      x => new UserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.SignInMethod,
        x.IpAddress,
        x.SignedInAt,
        x.LastActivityAt,
        currentSessionId.HasValue && x.Id == currentSessionId.Value),
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
