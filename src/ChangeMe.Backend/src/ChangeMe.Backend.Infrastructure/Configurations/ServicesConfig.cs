using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Email;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class ServicesConfig
{
  public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, ILogger logger)
  {
    services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IUserAuthTokenService, UserAuthTokenService>();
    services.AddScoped<IAuthEmailService, AuthEmailService>();
    services.AddSingleton<IPasswordPolicyValidator, PasswordPolicyValidator>();
    services.AddSingleton<IPasswordExpirationEvaluator, PasswordExpirationEvaluator>();
    services.AddScoped<UserInvitationService>();
    services.AddScoped<UserPasswordResetService>();
    services.AddScoped<UserEmailVerificationService>();
    services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
    services.AddSingleton<ISessionLifetimeService, SessionLifetimeService>();
    services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddScoped<IUserAccessor, UserAccessor>();
    logger.LogInformation("{Project} services configured", "Infrastructure");
    return services;
  }
}
