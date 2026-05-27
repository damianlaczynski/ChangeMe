using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class OidcPrincipalClaimsReader
{
  public static (string? Email, bool EmailVerified) ReadEmail(
    ClaimsPrincipal principal,
    bool trustIdpEmailWithoutEmailVerified = false)
  {
    var verifiedPrimary = FindClaimValue(principal, "verified_primary_email");
    if (!string.IsNullOrWhiteSpace(verifiedPrimary))
      return (verifiedPrimary.Trim(), true);

    var verifiedSecondary = FindClaimValue(principal, "verified_secondary_email");
    if (!string.IsNullOrWhiteSpace(verifiedSecondary))
      return (verifiedSecondary.Trim(), true);

    var email = FindClaimValue(principal, JwtRegisteredClaimNames.Email, ClaimTypes.Email);
    if (!string.IsNullOrWhiteSpace(email))
    {
      var verified = IsStandardEmailVerified(principal) || trustIdpEmailWithoutEmailVerified;
      return (email.Trim(), verified);
    }

    if (!trustIdpEmailWithoutEmailVerified)
      return (null, false);

    var preferredUsername = FindClaimValue(principal, "preferred_username");
    if (LooksLikeEmail(preferredUsername))
      return (preferredUsername!.Trim(), true);

    var upn = FindClaimValue(principal, "upn", ClaimTypes.Upn);
    if (LooksLikeEmail(upn))
      return (upn!.Trim(), true);

    var uniqueName = FindClaimValue(principal, "unique_name");
    if (LooksLikeEmail(uniqueName))
      return (uniqueName!.Trim(), true);

    return (null, false);
  }

  public static (string? FirstName, string? LastName) ReadName(ClaimsPrincipal principal)
  {
    var firstName = FindClaimValue(principal, JwtRegisteredClaimNames.GivenName, ClaimTypes.GivenName);
    var lastName = FindClaimValue(principal, JwtRegisteredClaimNames.FamilyName, ClaimTypes.Surname);

    if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
      return (firstName?.Trim(), lastName?.Trim());

    var fullName = FindClaimValue(principal, JwtRegisteredClaimNames.Name, ClaimTypes.Name);
    if (string.IsNullOrWhiteSpace(fullName))
      return (null, null);

    return SplitFullName(fullName.Trim());
  }

  private static (string? FirstName, string? LastName) SplitFullName(string fullName)
  {
    var spaceIndex = fullName.IndexOf(' ');
    if (spaceIndex <= 0)
      return (fullName, string.Empty);

    return (fullName[..spaceIndex].Trim(), fullName[(spaceIndex + 1)..].Trim());
  }

  private static bool LooksLikeEmail(string? value) =>
    !string.IsNullOrWhiteSpace(value) && value.Contains('@', StringComparison.Ordinal);

  private static bool IsStandardEmailVerified(ClaimsPrincipal principal)
  {
    var emailVerified = FindClaimValue(principal, "email_verified");
    return bool.TryParse(emailVerified, out var verified) && verified;
  }

  private static string? FindClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
  {
    foreach (var claimType in claimTypes)
    {
      var value = principal.FindFirstValue(claimType);
      if (!string.IsNullOrWhiteSpace(value))
        return value;
    }

    return null;
  }
}
