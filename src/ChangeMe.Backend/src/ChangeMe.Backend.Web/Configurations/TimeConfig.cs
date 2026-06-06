namespace ChangeMe.Backend.Web.Configurations;

public static class TimeConfig
{
  public static IServiceCollection AddTime(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
  {
    logger.LogInformation("{Module} services configured", "Time");
    return services;
  }
}
