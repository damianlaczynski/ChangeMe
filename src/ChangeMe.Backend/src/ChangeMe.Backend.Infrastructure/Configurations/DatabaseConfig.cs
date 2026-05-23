using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public sealed class DatabaseOptions
{
  public const string SectionName = "Database";

  public bool ApplyMigrationsOnStartup { get; set; }
}

public static class DatabaseConfig
{
  public static IServiceCollection AddDatabase(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
  {
    services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      var keys = string.Join(", ",
          builder.Configuration.GetSection("ConnectionStrings").GetChildren().Select(c => c.Key));
      throw new InvalidOperationException(
          $"Connection string 'DefaultConnection' is not configured. Available connection string keys: {keys}");
    }

#if PostgreSQL
    logger.LogInformation("Using PostgreSQL database");

    services.AddDbContext<ApplicationDbContext>(options =>
    {
      options.UseNpgsql(connectionString, npgsql =>
          npgsql.MigrationsHistoryTable("__EFMigrationsHistory", DatabaseSchema.Default));

      if (builder.Environment.IsDevelopment())
      {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        logger.LogInformation("Sensitive data logging enabled for development");
      }
    });

    services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres", tags: ["db", "ready"]);

    logger.LogInformation("PostgreSQL database connection configured");
#else
    logger.LogInformation("Using SQL Server database");

    services.AddDbContext<ApplicationDbContext>(options =>
    {
      options.UseSqlServer(connectionString, sql =>
          sql.MigrationsHistoryTable("__EFMigrationsHistory", DatabaseSchema.Default));

      if (builder.Environment.IsDevelopment())
      {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        logger.LogInformation("Sensitive data logging enabled for development");
      }
    });

    services.AddHealthChecks()
        .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "ready"]);

    logger.LogInformation("SQL Server database connection configured");
#endif
    return services;
  }

  public static async Task UseDatabase(this WebApplication app)
  {
    var databaseOptions = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    if (!databaseOptions.ApplyMigrationsOnStartup)
      return;

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await InitializeDatabaseAsync(
      db,
      scope.ServiceProvider.GetRequiredService<IConfiguration>(),
      scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
      scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>(),
      CancellationToken.None);
  }

  public static async Task InitializeDatabaseAsync(
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    IPasswordHasher passwordHasher,
    ILogger logger,
    CancellationToken cancellationToken = default)
  {
    await ApplyPendingMigrationsAsync(dbContext, cancellationToken);
    await ApplicationDataSeeder.SeedAsync(dbContext, configuration, passwordHasher, logger, cancellationToken);
  }

  public static async Task ApplyPendingMigrationsAsync(
    ApplicationDbContext dbContext,
    CancellationToken cancellationToken = default)
  {
    if (!dbContext.Database.GetMigrations().Any())
    {
      throw new InvalidOperationException(
        """
        No EF Core migrations were found in the Infrastructure assembly. MigrateAsync() succeeds with zero migrations but creates no tables.
        Add an initial migration from the solution root (see docs/database-and-docker.md):
          dotnet tool restore
          dotnet ef migrations add InitialCreate --project src/ChangeMe.Backend/src/ChangeMe.Backend.Infrastructure/ChangeMe.Backend.Infrastructure.csproj --startup-project src/ChangeMe.Backend/src/ChangeMe.Backend.Web/ChangeMe.Backend.Web.csproj --output-dir Persistence/Migrations
        Then set Database:ApplyMigrationsOnStartup to true in appsettings.Development.json, or run dotnet ef database update.
        """);
    }

    await dbContext.Database.MigrateAsync(cancellationToken);
  }

}
