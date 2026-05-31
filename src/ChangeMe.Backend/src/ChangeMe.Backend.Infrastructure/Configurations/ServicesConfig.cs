using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Auth.Passkey;
using ChangeMe.Backend.Infrastructure.Email;
using ChangeMe.Backend.Infrastructure.FileStorage;

namespace ChangeMe.Backend.Infrastructure.Configurations;

public static class ServicesConfig
{
  public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, ILogger logger)
  {
    services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
    services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IUserAuthTokenService, UserAuthTokenService>();
    services.AddScoped<IAuthEmailService, AuthEmailService>();
    services.AddSingleton<IPasswordPolicyValidator, PasswordPolicyValidator>();
    services.AddSingleton<IPasswordExpirationEvaluator, PasswordExpirationEvaluator>();
    services.AddSingleton<ITwoFactorPolicyEvaluator, TwoFactorPolicyEvaluator>();
    services.AddSingleton<IPasskeyPolicyEvaluator, PasskeyPolicyEvaluator>();
    services.AddScoped<IPasskeyFido2Service, PasskeyFido2Service>();
    services.AddSingleton<ITotpService, TotpService>();
    services.AddSingleton<ITwoFactorSecretProtector, TwoFactorSecretProtector>();
    services.AddSingleton<IRecoveryCodeHasher, RecoveryCodeHasher>();
    services.AddDataProtection();
    services.AddScoped<UserInvitationService>();
    services.AddScoped<UserPasswordResetService>();
    services.AddScoped<UserEmailVerificationService>();
    services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
    services.AddSingleton<ISessionLifetimeService, SessionLifetimeService>();
    services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddHttpClient(nameof(OidcExternalAuthService));
    services.AddScoped<IOidcExternalAuthService, OidcExternalAuthService>();
    services.AddScoped<IUserAccessor, UserAccessor>();
    services.AddSingleton<FileContentInspectorProvider>();
    services.AddSingleton<IFileContentValidator, FileContentValidator>();
    services.AddScoped<AttachmentUploadCoordinator>();
    services.AddScoped<IFileStorageService, LocalFileStorageService>();
    logger.LogInformation("{Project} services configured", "Infrastructure");
    return services;
  }
}
