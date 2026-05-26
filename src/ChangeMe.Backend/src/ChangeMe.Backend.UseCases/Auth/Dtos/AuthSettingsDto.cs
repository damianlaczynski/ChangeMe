namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record AuthSettingsDto
{
  public PasswordPolicySettingsDto PasswordPolicy { get; init; } = new();
  public bool PublicRegistrationEnabled { get; init; } = true;
  public bool EmailVerificationEnabled { get; init; }
  public bool PasswordExpirationEnabled { get; init; }
  public int MaximumPasswordAgeDays { get; init; } = 90;
  public bool TwoFactorAuthenticationEnabled { get; init; }
  public bool TwoFactorAuthenticationRequired { get; init; }
  public bool TrustIdentityProviderMfa { get; init; }
  public bool ExternalProvidersEnabled { get; init; }
  public TwoFactorSettingsDto TwoFactor { get; init; } = new();
  public IReadOnlyList<ExternalProviderSettingsDto> ExternalProviders { get; init; } = [];
  public PasskeySettingsDto Passkeys { get; init; } = new();
}

public sealed record TwoFactorSettingsDto
{
  public int VerificationCodeLength { get; init; } = 6;
  public int RecoveryCodeCount { get; init; } = 10;
  public int TotpTimeStepSeconds { get; init; } = 30;
  public int StepUpExternalSignInValidityMinutes { get; init; } = 15;
}

public sealed record ExternalProviderSettingsDto
{
  public string ProviderKey { get; init; } = string.Empty;
  public string DisplayName { get; init; } = string.Empty;
}

public sealed record PasswordPolicySettingsDto
{
  public int MinimumLength { get; init; }
  public int MaximumLength { get; init; }
  public bool RequireUppercase { get; init; }
  public bool RequireLowercase { get; init; }
  public bool RequireDigit { get; init; }
  public bool RequireSpecialCharacter { get; init; }
}
