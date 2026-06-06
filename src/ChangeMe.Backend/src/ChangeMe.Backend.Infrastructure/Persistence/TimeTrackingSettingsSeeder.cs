using ChangeMe.Backend.Domain.Aggregates.Time;

namespace ChangeMe.Backend.Infrastructure.Persistence;

public static class TimeTrackingSettingsSeeder
{
  public static async Task EnsureTimeTrackingSettingsAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken)
  {
    var exists = await context.TimeTrackingSettings
      .AnyAsync(x => x.Id == TimeTrackingSettings.SingletonId, cancellationToken);

    if (exists)
      return;

    await context.TimeTrackingSettings.AddAsync(TimeTrackingSettings.CreateDefault(), cancellationToken);
  }
}
