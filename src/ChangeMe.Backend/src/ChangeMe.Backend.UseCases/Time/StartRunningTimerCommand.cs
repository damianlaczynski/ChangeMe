using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record StartRunningTimerCommand(
  Guid? ProjectId,
  Guid? IssueId,
  bool ReplaceExisting = false) : ICommand<RunningTimerDto>;

public class StartRunningTimerHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<StartRunningTimerCommand, RunningTimerDto>
{
  public async Task<Result<RunningTimerDto>> Handle(
    StartRunningTimerCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.TimeLogOwn);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    if (command.ProjectId.HasValue)
    {
      var projectAccess = await TimeUtils.EnsureCanLogOnProjectAsync(
        context,
        command.ProjectId.Value,
        currentUserId,
        cancellationToken);
      if (!projectAccess.IsSuccess)
        return projectAccess.Map();
    }

    if (command.IssueId.HasValue)
    {
      if (!command.ProjectId.HasValue)
      {
        return Result.Invalid(
          new ValidationError(nameof(command.ProjectId), TimeConstraints.ProjectRequiredMessage));
      }

      var issueValidation = await TimeUtils.ValidateIssueBelongsToProjectAsync(
        context,
        command.ProjectId.Value,
        command.IssueId,
        cancellationToken);
      if (!issueValidation.IsSuccess)
        return issueValidation.Map();
    }

    var utcNow = DateTime.UtcNow;
    var existingTimer = await context.UserRunningTimers
      .FirstOrDefaultAsync(t => t.UserId == currentUserId, cancellationToken);

    if (existingTimer is not null && !command.ReplaceExisting)
      return Result.Conflict(TimeConstraints.TimerAlreadyRunningMessage);

    UserRunningTimer timer;
    if (existingTimer is null)
    {
      timer = UserRunningTimer.Start(currentUserId, command.ProjectId, command.IssueId, utcNow);
      await context.UserRunningTimers.AddAsync(timer, cancellationToken);
    }
    else
    {
      existingTimer.Restart(command.ProjectId, command.IssueId, utcNow);
      timer = existingTimer;
    }

    await context.SaveChangesAsync(cancellationToken);

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

    var elapsedMinutes = timer.GetElapsedWholeMinutes(utcNow);

    return Result.Success(new RunningTimerDto
    {
      ProjectId = timer.ProjectId,
      ProjectName = projectName,
      IssueId = timer.IssueId,
      IssueTitle = issueTitle,
      StartedAtUtc = timer.StartedAtUtc,
      ElapsedMinutes = elapsedMinutes,
      ElapsedFormatted = TimeUtils.FormatDuration(elapsedMinutes),
    });
  }
}
