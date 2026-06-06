using ChangeMe.Backend.UseCases.Billing.Services;
using ChangeMe.Backend.UseCases.Issues.Services;
using ChangeMe.Backend.UseCases.Notifications.Services;
using ChangeMe.Backend.Web.Notifications;
using Hangfire;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Web.Configurations;

public static class NotificationsConfig
{
  public static IServiceCollection AddNotifications(this IServiceCollection services, WebApplicationBuilder builder, Microsoft.Extensions.Logging.ILogger logger)
  {
    services.Configure<NotificationRetentionOptions>(builder.Configuration.GetSection(NotificationRetentionOptions.SectionName));
    services.AddSignalR();
    services.AddSingleton(TimeProvider.System);
    services.AddScoped<IssueNotificationService>();
    services.AddScoped<AvailabilityNotificationService>();
    services.AddScoped<NotificationRetentionPolicy>();
    services.AddScoped<NotificationRetentionCleanupJob>();
    services.AddSingleton<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();

    logger.LogInformation("{Project} services configured", "Notifications");
    return services;
  }

  public static WebApplication UseNotifications(this WebApplication app)
  {
    app.MapHub<NotificationHub>("/hubs/notifications");

    var retentionOptions = app.Services.GetRequiredService<IOptions<NotificationRetentionOptions>>().Value;
    var cleanupCronExpression = string.IsNullOrWhiteSpace(retentionOptions.CleanupCronExpression)
      ? "0 3 * * *"
      : retentionOptions.CleanupCronExpression;

    RecurringJob.AddOrUpdate<NotificationRetentionCleanupJob>(
      "notifications-retention-cleanup",
      job => job.ExecuteAsync(JobCancellationToken.Null),
      cleanupCronExpression);

    return app;
  }
}
