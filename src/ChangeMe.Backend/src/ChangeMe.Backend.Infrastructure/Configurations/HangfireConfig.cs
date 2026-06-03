using Hangfire;
#if PostgreSQL
using Hangfire.PostgreSql;
#else
using Hangfire.SqlServer;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public sealed class HangfireOptions
{
  public const string SectionName = nameof(HangfireOptions);

  public string DashboardPath { get; set; } = "/hangfire";
}

public static class HangfireConfig
{
  public static IServiceCollection AddHangfire(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
  {
    services.Configure<HangfireOptions>(builder.Configuration.GetSection(HangfireOptions.SectionName));

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      var keys = string.Join(", ",
          builder.Configuration.GetSection("ConnectionStrings").GetChildren().Select(c => c.Key));
      throw new InvalidOperationException(
          $"Connection string 'DefaultConnection' is not configured. Available connection string keys: {keys}");
    }

    services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
#if PostgreSQL
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
#else
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions()));
#endif

    services.AddHangfireServer();

    logger.LogInformation("{Project} services configured", "Hangfire");
    return services;
  }

  public static WebApplication UseHangfireDashboard(this WebApplication app)
  {
    var options = app.Services.GetRequiredService<IOptions<HangfireOptions>>().Value;
    app.UseHangfireDashboard(options?.DashboardPath ?? "/hangfire");
    return app;
  }
}
