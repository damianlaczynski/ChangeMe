using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class AuthOptions
{
  public const string SectionName = "Auth";

  public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

  public JwtOptions Jwt { get; set; } = new();

  public PasswordPolicyOptions PasswordPolicy { get; set; } = new();

  public PasswordExpirationOptions PasswordExpiration { get; set; } = new();

  public EmailVerificationOptions EmailVerification { get; set; } = new();

  public RegistrationOptions Registration { get; set; } = new();

  public PasswordResetOptions PasswordReset { get; set; } = new();

  public InvitationOptions Invitations { get; set; } = new();

  public TwoFactorOptions TwoFactor { get; set; } = new();

  public ExternalAuthOptions External { get; set; } = new();

  public PasskeyOptions Passkeys { get; set; } = new();
}

public sealed class PasswordExpirationOptions
{
  public bool Enabled { get; set; }

  public int MaximumPasswordAgeDays { get; set; } = 90;
}

public sealed class EmailVerificationOptions
{
  public bool Enabled { get; set; }

  public int LinkLifetimeHours { get; set; } = 72;
}

public sealed class RegistrationOptions
{
  public bool PublicEnabled { get; set; } = true;
}

public sealed class PasswordResetOptions
{
  public int LinkLifetimeHours { get; set; } = 24;
}

public sealed class TwoFactorOptions
{
  public bool Enabled { get; set; }

  public bool Required { get; set; }

  public bool TrustIdentityProviderMfa { get; set; }

  public int TotpTimeStepSeconds { get; set; } = 30;

  public int TotpValidationWindowSteps { get; set; } = 1;

  public int VerificationCodeLength { get; set; } = 6;

  public int RecoveryCodeCount { get; set; } = 10;

  public int PendingSignInChallengeLifetimeMinutes { get; set; } = 10;

  public int MaxFailedVerificationAttempts { get; set; } = 5;

  public int StepUpExternalSignInValidityMinutes { get; set; } = 15;

  public string TotpIssuerName { get; set; } = "ChangeMe";
}

public sealed class ExternalAuthOptions
{
  public bool Enabled { get; set; }

  public int PendingLifetimeMinutes { get; set; } = 10;

  public string SignInCallbackPath { get; set; } = "/external-sign-in/callback";

  public List<ExternalProviderOptions> Providers { get; set; } = [];
}

public sealed class ExternalProviderOptions
{
  public string ProviderKey { get; set; } = string.Empty;

  public string DisplayName { get; set; } = string.Empty;

  public string Authority { get; set; } = string.Empty;

  public string ClientId { get; set; } = string.Empty;

  public string ClientSecret { get; set; } = string.Empty;

  public List<string> AllowedEmailDomains { get; set; } = [];

  /// <summary>
  /// Discovery (default): validate ID token issuer from OIDC metadata.
  /// MicrosoftMultiTenant: accept any Microsoft Entra tenant issuer (use with /common or /organizations authority).
  /// </summary>
  public string IssuerValidationMode { get; set; } = "Discovery";

  /// <summary>
  /// When true, treat the provider email claim as verified even without email_verified (common for Microsoft Entra).
  /// </summary>
  public bool TrustIdpEmailWithoutEmailVerified { get; set; }

  public bool IsConfigured =>
    !string.IsNullOrWhiteSpace(ProviderKey)
    && !string.IsNullOrWhiteSpace(DisplayName)
    && !string.IsNullOrWhiteSpace(Authority)
    && !string.IsNullOrWhiteSpace(ClientId)
    && !string.IsNullOrWhiteSpace(ClientSecret);
}

public sealed class InvitationOptions
{
  public int InvitationLinkLifetimeHours { get; set; } = 72;

  public InvitationRetentionSettings Retention { get; set; } = new();
}

public sealed class InvitationRetentionSettings
{
  public int RevokedInvitationRetentionDays { get; set; } = 7;

  public string CleanupCronExpression { get; set; } = "0 4 * * *";
}

public sealed class PasskeyOptions
{
  public bool PasskeysAuthenticationEnabled { get; set; }

  public bool PasskeysAuthenticationRequired { get; set; }

  public bool PasskeySatisfiesTwoFactor { get; set; }

  public bool AllowPasskeyOnlyAccounts { get; set; }

  public bool DiscoverablePasskeySignInOnLogin { get; set; } = true;

  public bool OfferPasskeyEnrollmentPrompt { get; set; }

  public string? RelyingPartyId { get; set; }

  public string RelyingPartyDisplayName { get; set; } = "ChangeMe";

  public int MaximumPasskeysPerUser { get; set; } = 10;

  public int ChallengeLifetimeMinutes { get; set; } = 5;

  public bool UserVerificationRequired { get; set; } = true;

  public string AllowedAuthenticatorAttachment { get; set; } = "Any";

  public string AttestationConveyance { get; set; } = "None";

  public int PasskeyStepUpValidityMinutes { get; set; } = 15;

  public int MaxFailedPasskeyAttempts { get; set; } = 5;
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
