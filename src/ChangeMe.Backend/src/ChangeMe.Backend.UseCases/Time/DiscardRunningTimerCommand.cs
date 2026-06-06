using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record DiscardRunningTimerCommand() : ICommand<bool>;

public class DiscardRunningTimerHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DiscardRunningTimerCommand, bool>
{
  public async Task<Result<bool>> Handle(DiscardRunningTimerCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var timer = await context.UserRunningTimers
      .FirstOrDefaultAsync(t => t.UserId == currentUserId, cancellationToken);

    if (timer is null)
      return Result.NotFound(TimeConstraints.TimerNotRunningMessage);

    context.UserRunningTimers.Remove(timer);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}
