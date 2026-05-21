using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Email;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class ServicesConfig
{
  public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, ILogger logger)
  {
    services.Configure<EmailOptions>(configuration.GetSection("Email"));

    services.AddScoped<IEmailService, EmailService>();
    services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
    services.Configure<SessionOptions>(configuration.GetSection("Session"));
    services.AddSingleton<ISessionLifetimeService, SessionLifetimeService>();
    services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddScoped<IUserAccessor, UserAccessor>();
    logger.LogInformation("{Project} services configured", "Infrastructure");
    return services;
  }
}
