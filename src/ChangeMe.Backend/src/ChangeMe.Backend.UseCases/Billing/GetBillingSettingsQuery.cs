using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetBillingSettingsQuery : IQuery<BillingSettingsDto>;

public class GetBillingSettingsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetBillingSettingsQuery, BillingSettingsDto>
{
  public async Task<Result<BillingSettingsDto>> Handle(
    GetBillingSettingsQuery query,
    CancellationToken cancellationToken)
  {
    if (!BillingUtils.CanViewBillingReports(userAccessor))
      return Result.Forbidden();

    var settings = await context.BillingSettings
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
    if (settings is null)
      return Result.NotFound();

    var canEdit = BillingUtils.CanManageSettlements(userAccessor);
    return Result.Success(BillingSettingsUtils.MapDto(settings, canEdit));
  }
}
