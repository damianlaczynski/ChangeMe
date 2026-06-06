using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class PositionsUtils
{
  public static async Task<Result> EnsureUniquePositionNameAsync(
    ApplicationDbContext context,
    string name,
    Guid? excludePositionId,
    CancellationToken cancellationToken)
  {
    var normalizedName = Position.NormalizeName(name);
    var exists = await context.Positions
      .AsNoTracking()
      .AnyAsync(
        p => p.NormalizedName == normalizedName && (!excludePositionId.HasValue || p.Id != excludePositionId.Value),
        cancellationToken);

    return exists
      ? Result.Conflict(BillingConstraints.PositionNameDuplicateMessage)
      : Result.Success();
  }
}
