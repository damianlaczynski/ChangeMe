using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class OidcIssuerValidation
{
  internal static void Configure(
    TokenValidationParameters validationParameters,
    OidcIssuerValidationMode mode,
    OpenIdConnectConfiguration configuration)
  {
    if (mode == OidcIssuerValidationMode.MicrosoftMultiTenant)
    {
      validationParameters.IssuerValidator = (issuer, _, _) =>
        IsValidMicrosoftIssuer(issuer)
          ? issuer
          : throw new SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
      return;
    }

    validationParameters.ValidIssuer = configuration.Issuer;
  }

  public static bool IsValidMicrosoftIssuer(string issuer)
  {
    if (!issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase)
        || !issuer.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase))
      return false;

    var tenantSegment = issuer["https://login.microsoftonline.com/".Length..^"/v2.0".Length];
    if (tenantSegment.Equals("common", StringComparison.OrdinalIgnoreCase)
        || tenantSegment.Equals("organizations", StringComparison.OrdinalIgnoreCase))
      return true;

    return Guid.TryParse(tenantSegment, out _);
  }
}
