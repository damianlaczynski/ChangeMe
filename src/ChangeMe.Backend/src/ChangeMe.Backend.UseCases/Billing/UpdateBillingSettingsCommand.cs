using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record UpdateBillingSettingsCommand(
  decimal DefaultAnnualLeaveDays,
  bool AllowHalfDayLeave,
  string DefaultWorkdayStart,
  string DefaultWorkdayEnd,
  string HalfDaySplitTime,
  IReadOnlyList<DayOfWeek> DefaultWorkdays,
  AvailabilityStatus DefaultAvailabilityStatus) : ICommand<BillingSettingsDto>;

public class UpdateBillingSettingsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateBillingSettingsCommand, BillingSettingsDto>
{
  public async Task<Result<BillingSettingsDto>> Handle(
    UpdateBillingSettingsCommand command,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanManageSettlements(userAccessor))
      return Result.Forbidden();

    var settings = await context.BillingSettings
      .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
    if (settings is null)
      return Result.NotFound();

    var timesResult = BillingSettingsUtils.ParseWorkdayTimes(
      command.DefaultWorkdayStart,
      command.DefaultWorkdayEnd,
      command.HalfDaySplitTime);
    if (!timesResult.IsSuccess)
      return timesResult.Map();

    var (start, end, split) = timesResult.Value;
    var updateResult = settings.Update(
      command.DefaultAnnualLeaveDays,
      command.AllowHalfDayLeave,
      start,
      end,
      split,
      command.DefaultWorkdays,
      command.DefaultAvailabilityStatus);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(BillingSettingsUtils.MapDto(settings, canEdit: true));
  }
}
