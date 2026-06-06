using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence;

public static class BillingSettingsSeeder
{
  public static async Task EnsureBillingSettingsAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken)
  {
    var exists = await context.BillingSettings
      .AnyAsync(x => x.Id == BillingSettings.SingletonId, cancellationToken);

    if (exists)
      return;

    await context.BillingSettings.AddAsync(BillingSettings.CreateDefault(), cancellationToken);
  }
}

public static class LeaveTypeSeeder
{
  public static async Task EnsureLeaveTypesAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken)
  {
    if (await context.LeaveTypes.AnyAsync(cancellationToken))
      return;

    var seededTypes = new[]
    {
      LeaveType.CreateSeeded("Vacation", "VAC", countsAsPaid: true, usesAllowance: true, requiresApproval: true),
      LeaveType.CreateSeeded("Sick leave", "SICK", countsAsPaid: true, usesAllowance: false, requiresApproval: true),
      LeaveType.CreateSeeded("Unpaid leave", "UNPD", countsAsPaid: false, usesAllowance: false, requiresApproval: true),
      LeaveType.CreateSeeded("Other paid", "OTH", countsAsPaid: true, usesAllowance: false, requiresApproval: true),
    };

    await context.LeaveTypes.AddRangeAsync(seededTypes, cancellationToken);
  }
}
