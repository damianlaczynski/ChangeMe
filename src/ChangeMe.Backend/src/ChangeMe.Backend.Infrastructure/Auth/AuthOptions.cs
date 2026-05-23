using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class AuthOptions
{
  public const string SectionName = "Auth";

  public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

  public JwtOptions Jwt { get; set; } = new();

  public AuthSessionOptions Session { get; set; } = new();

  public PasswordPolicyOptions PasswordPolicy { get; set; } = new();

  public bool PasswordExpirationEnabled { get; set; }

  public int MaximumPasswordAgeDays { get; set; } = 90;

  public bool EmailVerificationEnabled { get; set; }

  public int EmailVerificationLinkLifetimeHours { get; set; } = 72;

  public bool PublicRegistrationEnabled { get; set; } = true;

  public int PasswordResetLinkLifetimeHours { get; set; } = 24;

  public int InvitationLinkLifetimeHours { get; set; } = 72;
}

public sealed class AuthSessionOptions
{
  public int PersistentSessionLifetimeDays { get; set; } = 14;

  public int BrowserSessionLifetimeDays { get; set; } = 1;
}

public sealed class PasswordPolicyOptions
{
  public int MinimumLength { get; set; } = UserConstraints.PASSWORD_MIN_LENGTH;

  public int MaximumLength { get; set; } = UserConstraints.PASSWORD_MAX_LENGTH;

  public bool RequireUppercase { get; set; } = true;

  public bool RequireLowercase { get; set; } = true;

  public bool RequireDigit { get; set; } = true;

  public bool RequireSpecialCharacter { get; set; }
}
