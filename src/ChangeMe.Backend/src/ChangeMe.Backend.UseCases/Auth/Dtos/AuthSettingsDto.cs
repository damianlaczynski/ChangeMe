namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record AuthSettingsDto
{
  public PasswordPolicySettingsDto PasswordPolicy { get; init; } = new();
  public bool PublicRegistrationEnabled { get; init; } = true;
  public bool EmailVerificationEnabled { get; init; }
  public bool PasswordExpirationEnabled { get; init; }
  public int MaximumPasswordAgeDays { get; init; } = 90;
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
