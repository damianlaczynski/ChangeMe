using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record DeleteLeaveRequestCommand(Guid Id) : ICommand<bool>;

public class DeleteLeaveRequestHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeleteLeaveRequestCommand, bool>
{
  public async Task<Result<bool>> Handle(
    DeleteLeaveRequestCommand command,
    CancellationToken cancellationToken)
  {
    var request = await context.LeaveRequests
      .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);
    if (request is null)
      return Result.NotFound();

    var detailsResult = await GetLeaveRequestByIdHandler.MapDetailsAsync(
      context,
      userAccessor,
      request,
      cancellationToken);
    if (!detailsResult.IsSuccess)
      return detailsResult.Map();

    if (!detailsResult.Value.CanDelete)
      return Result.Forbidden();

    context.LeaveRequests.Remove(request);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}
