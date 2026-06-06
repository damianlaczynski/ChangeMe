using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record GetRunningTimerQuery() : IQuery<RunningTimerStateDto>;

public class GetRunningTimerHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetRunningTimerQuery, RunningTimerStateDto>
{
  public async Task<Result<RunningTimerStateDto>> Handle(
    GetRunningTimerQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var timer = await context.UserRunningTimers
      .AsNoTracking()
      .FirstOrDefaultAsync(t => t.UserId == currentUserId, cancellationToken);

    if (timer is null)
      return Result.Success(new RunningTimerStateDto());

    var utcNow = DateTime.UtcNow;
    var elapsedMinutes = timer.GetElapsedWholeMinutes(utcNow);

    string? projectName = null;
    if (timer.ProjectId.HasValue)
    {
      projectName = await context.Projects
        .AsNoTracking()
        .Where(p => p.Id == timer.ProjectId.Value)
        .Select(p => p.Name)
        .FirstOrDefaultAsync(cancellationToken);
    }

    string? issueTitle = null;
    if (timer.IssueId.HasValue)
    {
      issueTitle = await context.Issues
        .AsNoTracking()
        .Where(i => i.Id == timer.IssueId.Value)
        .Select(i => i.Title)
        .FirstOrDefaultAsync(cancellationToken);
    }

    return Result.Success(new RunningTimerStateDto
    {
      Timer = new RunningTimerDto
      {
        ProjectId = timer.ProjectId,
        ProjectName = projectName,
        IssueId = timer.IssueId,
        IssueTitle = issueTitle,
        StartedAtUtc = timer.StartedAtUtc,
        ElapsedMinutes = elapsedMinutes,
        ElapsedFormatted = TimeUtils.FormatDuration(elapsedMinutes),
      },
    });
  }
}
