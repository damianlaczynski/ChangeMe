namespace ChangeMe.Backend.Infrastructure.Auth;

public enum OidcIssuerValidationMode
{
  Discovery = 0,
  MicrosoftMultiTenant = 1
}

public static class OidcIssuerValidationModeParser
{
  public static OidcIssuerValidationMode Parse(string? value) =>
    Enum.TryParse<OidcIssuerValidationMode>(value, ignoreCase: true, out var mode)
      ? mode
      : OidcIssuerValidationMode.Discovery;
}
