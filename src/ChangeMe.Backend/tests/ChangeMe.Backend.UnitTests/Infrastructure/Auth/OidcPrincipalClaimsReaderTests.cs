using System.Security.Claims;
using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class OidcPrincipalClaimsReaderTests
{
  [Fact]
  public void ReadEmail_ForMicrosoftVerifiedPrimaryEmail_ShouldTreatAsVerified()
  {
    var principal = CreatePrincipal(
      ("iss", "https://login.microsoftonline.com/tenant/v2.0"),
      ("verified_primary_email", "user@contoso.com"));

    var (email, verified) = OidcPrincipalClaimsReader.ReadEmail(principal, trustIdpEmailWithoutEmailVerified: true);

    Assert.Equal("user@contoso.com", email);
    Assert.True(verified);
  }

  [Fact]
  public void ReadEmail_ForMicrosoftPreferredUsername_ShouldTreatAsVerifiedWhenTrustEnabled()
  {
    var principal = CreatePrincipal(
      ("iss", "https://login.microsoftonline.com/tenant/v2.0"),
      ("preferred_username", "user@contoso.com"));

    var (email, verified) = OidcPrincipalClaimsReader.ReadEmail(principal, trustIdpEmailWithoutEmailVerified: true);

    Assert.Equal("user@contoso.com", email);
    Assert.True(verified);
  }

  [Fact]
  public void ReadEmail_ForMicrosoftEmailWithoutEmailVerified_ShouldTreatAsVerifiedWhenTrustEnabled()
  {
    var principal = CreatePrincipal(
      ("iss", "https://login.microsoftonline.com/9188040d-6ce8-4630-964d-3869bc8bd203/v2.0"),
      ("email", "user@outlook.com"));

    var (email, verified) = OidcPrincipalClaimsReader.ReadEmail(principal, trustIdpEmailWithoutEmailVerified: true);

    Assert.Equal("user@outlook.com", email);
    Assert.True(verified);
  }

  [Fact]
  public void ReadEmail_ForGoogleUnverifiedEmail_ShouldNotTreatAsVerified()
  {
    var principal = CreatePrincipal(
      ("iss", "https://accounts.google.com"),
      ("email", "user@gmail.com"),
      ("email_verified", "false"));

    var (email, verified) = OidcPrincipalClaimsReader.ReadEmail(principal);

    Assert.Equal("user@gmail.com", email);
    Assert.False(verified);
  }

  [Fact]
  public void ReadName_ForMicrosoftDisplayName_ShouldSplitIntoFirstAndLastName()
  {
    var principal = CreatePrincipal(
      ("iss", "https://login.microsoftonline.com/tenant/v2.0"),
      ("name", "Jan Kowalski"));

    var (firstName, lastName) = OidcPrincipalClaimsReader.ReadName(principal);

    Assert.Equal("Jan", firstName);
    Assert.Equal("Kowalski", lastName);
  }

  [Fact]
  public void ReadName_WhenGivenAndFamilyNamePresent_ShouldPreferStandardClaims()
  {
    var principal = CreatePrincipal(
      ("given_name", "Ada"),
      ("family_name", "Lovelace"),
      ("name", "Ignored Name"));

    var (firstName, lastName) = OidcPrincipalClaimsReader.ReadName(principal);

    Assert.Equal("Ada", firstName);
    Assert.Equal("Lovelace", lastName);
  }

  [Fact]
  public void ReadEmail_ForGoogleVerifiedEmail_ShouldTreatAsVerified()
  {
    var principal = CreatePrincipal(
      ("iss", "https://accounts.google.com"),
      ("email", "user@gmail.com"),
      ("email_verified", "true"));

    var (email, verified) = OidcPrincipalClaimsReader.ReadEmail(principal);

    Assert.Equal("user@gmail.com", email);
    Assert.True(verified);
  }

  private static ClaimsPrincipal CreatePrincipal(params (string Type, string Value)[] claims)
  {
    var identity = new ClaimsIdentity(claims.Select(x => new Claim(x.Type, x.Value)));
    return new ClaimsPrincipal(identity);
  }
}
