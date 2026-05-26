using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class OidcIssuerValidationTests
{
  [Fact]
  public void IsValidMicrosoftIssuer_ForTenantGuid_ShouldReturnTrue()
  {
    Assert.True(OidcIssuerValidation.IsValidMicrosoftIssuer(
      "https://login.microsoftonline.com/378d1f4a-4edf-429f-b213-5fe0ab8968b7/v2.0"));
  }

  [Theory]
  [InlineData("https://login.microsoftonline.com/common/v2.0")]
  [InlineData("https://login.microsoftonline.com/organizations/v2.0")]
  public void IsValidMicrosoftIssuer_ForCommonOrOrganizations_ShouldReturnTrue(string issuer)
  {
    Assert.True(OidcIssuerValidation.IsValidMicrosoftIssuer(issuer));
  }

  [Fact]
  public void IsValidMicrosoftIssuer_ForUnknownHost_ShouldReturnFalse()
  {
    Assert.False(OidcIssuerValidation.IsValidMicrosoftIssuer("https://accounts.google.com"));
  }
}
