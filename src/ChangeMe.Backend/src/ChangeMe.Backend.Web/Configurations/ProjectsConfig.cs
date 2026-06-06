using ChangeMe.Backend.UseCases.Projects.Services;

namespace ChangeMe.Backend.Web.Configurations;

public static class ProjectsConfig
{
  public static IServiceCollection AddProjects(this IServiceCollection services, Microsoft.Extensions.Logging.ILogger logger)
  {
    services.AddScoped<ProjectMembershipService>();
    logger.LogInformation("{Project} services configured", "Projects");
    return services;
  }
}
