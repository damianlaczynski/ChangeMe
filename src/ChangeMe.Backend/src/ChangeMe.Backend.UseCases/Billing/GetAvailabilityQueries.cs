using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetMyAvailabilityCalendarQuery(DateOnly From, DateOnly To)
  : IQuery<AvailabilityCalendarResultDto>;

public record GetTeamAvailabilityCalendarQuery(
  DateOnly From,
  DateOnly To,
  IReadOnlyList<Guid>? UserIds,
  IReadOnlyList<Guid>? ProjectIds,
  IReadOnlyList<string>? Statuses) : IQuery<AvailabilityCalendarResultDto>;

public record GetMyWeeklyPatternQuery() : IQuery<WeeklyRecurringPatternDto>;

public record GetUserWeeklyPatternQuery(Guid UserId) : IQuery<WeeklyRecurringPatternDto>;

public record GetAvailabilityDayQuery(Guid UserId, DateOnly Date) : IQuery<AvailabilityDayResultDto>;

public class GetMyAvailabilityCalendarHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetMyAvailabilityCalendarQuery, AvailabilityCalendarResultDto>
{
  public async Task<Result<AvailabilityCalendarResultDto>> Handle(
    GetMyAvailabilityCalendarQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (!userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
      return Result.Forbidden();

    return await AvailabilityCalendarQueryHelpers.BuildCalendarResultAsync(
      context,
      [userAccessor.UserId.Value],
      query.From,
      query.To,
      isTruncated: false,
      cancellationToken);
  }
}

public class GetTeamAvailabilityCalendarHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetTeamAvailabilityCalendarQuery, AvailabilityCalendarResultDto>
{
  public async Task<Result<AvailabilityCalendarResultDto>> Handle(
    GetTeamAvailabilityCalendarQuery query,
    CancellationToken cancellationToken)
  {
    if (!userAccessor.HasPermission(PermissionCodes.BillingViewAny))
      return Result.Forbidden();

    var usersQuery = context.Users
      .AsNoTracking()
      .Where(u => !u.Deactivated);

    if (query.UserIds is { Count: > 0 })
      usersQuery = usersQuery.Where(u => query.UserIds.Contains(u.Id));

    if (query.ProjectIds is { Count: > 0 })
    {
      usersQuery = usersQuery.Where(u =>
        context.ProjectMembers.Any(m =>
          query.ProjectIds.Contains(m.ProjectId) && m.UserId == u.Id));
    }

    var users = await usersQuery
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .Select(u => new AvailabilityCalendarUserDto
      {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        FullName = u.FirstName + " " + u.LastName,
      })
      .ToListAsync(cancellationToken);

    var isTruncated = false;
    if (query.UserIds is not { Count: > 0 } && users.Count > AvailabilityUtils.TeamCalendarUserCap)
    {
      users = users.Take(AvailabilityUtils.TeamCalendarUserCap).ToList();
      isTruncated = true;
    }

    var userIds = users.Select(u => u.Id).ToList();
    if (userIds.Count == 0)
    {
      return Result.Success(new AvailabilityCalendarResultDto
      {
        From = query.From,
        To = query.To,
        Users = [],
        Entries = [],
        IsTruncated = isTruncated,
      });
    }

    var entries = await AvailabilityCalendarQueryHelpers.LoadEntriesAsync(
      context, userIds, query.From, query.To, query.Statuses, cancellationToken);

    return Result.Success(new AvailabilityCalendarResultDto
    {
      From = query.From,
      To = query.To,
      Users = users,
      Entries = entries,
      IsTruncated = isTruncated,
    });
  }
}

public class GetMyWeeklyPatternHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetMyWeeklyPatternQuery, WeeklyRecurringPatternDto>
{
  public async Task<Result<WeeklyRecurringPatternDto>> Handle(
    GetMyWeeklyPatternQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (!userAccessor.HasPermission(PermissionCodes.BillingViewOwn))
      return Result.Forbidden();

    return await GetUserWeeklyPatternHandler.LoadPatternAsync(
      context,
      userAccessor,
      userAccessor.UserId.Value,
      cancellationToken);
  }
}

public class GetUserWeeklyPatternHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetUserWeeklyPatternQuery, WeeklyRecurringPatternDto>
{
  public async Task<Result<WeeklyRecurringPatternDto>> Handle(
    GetUserWeeklyPatternQuery query,
    CancellationToken cancellationToken) =>
    await LoadPatternAsync(context, userAccessor, query.UserId, cancellationToken);

  internal static async Task<Result<WeeklyRecurringPatternDto>> LoadPatternAsync(
    ApplicationDbContext context,
    IUserAccessor userAccessor,
    Guid userId,
    CancellationToken cancellationToken)
  {
    if (!AvailabilityUtils.CanViewUserAvailability(userAccessor, userId))
      return Result.Forbidden();

    var pattern = await context.WeeklyRecurringPatterns
      .AsNoTracking()
      .Include(p => p.Days)
      .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    var dto = AvailabilityUtils.MapPattern(pattern);
    dto.UserId = userId;
    dto.CanEdit = AvailabilityUtils.CanManageUserAvailability(userAccessor, userId);
    return Result.Success(dto);
  }
}

public class GetAvailabilityDayHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetAvailabilityDayQuery, AvailabilityDayResultDto>
{
  public async Task<Result<AvailabilityDayResultDto>> Handle(
    GetAvailabilityDayQuery query,
    CancellationToken cancellationToken)
  {
    if (!AvailabilityUtils.CanViewUserAvailability(userAccessor, query.UserId))
      return Result.Forbidden();

    var entries = await context.AvailabilityEntries
      .AsNoTracking()
      .Where(e => e.UserId == query.UserId
                  && e.StartDate <= query.Date
                  && e.EndDate >= query.Date)
      .ToListAsync(cancellationToken);

    return Result.Success(new AvailabilityDayResultDto
    {
      Date = query.Date,
      UserId = query.UserId,
      Entries = AvailabilityUtils.GetEntriesForDay(
        entries.Select(AvailabilityUtils.MapEntry).ToList(),
        query.Date),
      CanManage = AvailabilityUtils.CanManageUserAvailability(userAccessor, query.UserId),
    });
  }
}

internal static class AvailabilityCalendarQueryHelpers
{
  internal static async Task<Result<AvailabilityCalendarResultDto>> BuildCalendarResultAsync(
    ApplicationDbContext context,
    IReadOnlyList<Guid> userIds,
    DateOnly from,
    DateOnly to,
    bool isTruncated,
    CancellationToken cancellationToken)
  {
    var users = await context.Users
      .AsNoTracking()
      .Where(u => userIds.Contains(u.Id))
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .Select(u => new AvailabilityCalendarUserDto
      {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        FullName = u.FirstName + " " + u.LastName,
      })
      .ToListAsync(cancellationToken);

    var entries = await LoadEntriesAsync(context, userIds, from, to, statuses: null, cancellationToken);

    return Result.Success(new AvailabilityCalendarResultDto
    {
      From = from,
      To = to,
      Users = users,
      Entries = entries,
      IsTruncated = isTruncated,
    });
  }

  internal static async Task<List<AvailabilityEntryDto>> LoadEntriesAsync(
    ApplicationDbContext context,
    IReadOnlyList<Guid> userIds,
    DateOnly from,
    DateOnly to,
    IReadOnlyList<string>? statuses,
    CancellationToken cancellationToken)
  {
    var entries = await context.AvailabilityEntries
      .AsNoTracking()
      .Where(e => userIds.Contains(e.UserId)
                  && e.StartDate <= to
                  && e.EndDate >= from)
      .ToListAsync(cancellationToken);

    var mapped = entries.Select(AvailabilityUtils.MapEntry).ToList();
    if (statuses is not { Count: > 0 })
      return mapped;

    return mapped
      .Where(e => statuses.Contains(MapStatusFilter(e)))
      .ToList();
  }

  private static string MapStatusFilter(AvailabilityEntryDto entry) =>
    entry.Source == Domain.Aggregates.Billing.Enums.AvailabilityEntrySource.Leave
      ? "Leave"
      : entry.Status.ToString();
}
