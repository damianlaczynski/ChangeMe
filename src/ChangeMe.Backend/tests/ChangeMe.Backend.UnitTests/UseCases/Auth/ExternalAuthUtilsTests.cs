using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class ExternalAuthUtilsTests
{
  [Fact]
  public void IsEmailDomainAllowed_WhenNoDomainsConfigured_ShouldAllowAnyEmail()
  {
    var allowed = ExternalAuthUtils.IsEmailDomainAllowed("user@anywhere.com", []);
    Assert.True(allowed);
  }

  [Fact]
  public void IsEmailDomainAllowed_WhenDomainMatches_ShouldReturnTrue()
  {
    var allowed = ExternalAuthUtils.IsEmailDomainAllowed(
      "user@Example.com",
      ["example.com"]);
    Assert.True(allowed);
  }

  [Fact]
  public void IsEmailDomainAllowed_WhenDomainDoesNotMatch_ShouldReturnFalse()
  {
    var allowed = ExternalAuthUtils.IsEmailDomainAllowed(
      "user@other.com",
      ["example.com"]);
    Assert.False(allowed);
  }

  [Fact]
  public void IsInvitationPending_WhenInvitationWasSent_ShouldReturnTrue()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("invited@example.com").Value;
    user.RecordInvitationIssued(utcNow, utcNow.AddHours(72));

    Assert.True(ExternalAuthUtils.IsInvitationPending(user));
    Assert.False(ExternalAuthUtils.IsExternalOnlyAccount(user));
  }

  [Fact]
  public void IsExternalOnlyAccount_WhenExternalLoginExistsWithoutInvitation_ShouldReturnTrue()
  {
    var user = User.CreateInvited("external-only@example.com").Value;
    var login = ExternalLogin.Create(user.Id, "microsoft", "subject-1").Value;
    user.AddExternalLogin(login);

    Assert.False(ExternalAuthUtils.IsInvitationPending(user));
    Assert.True(ExternalAuthUtils.IsExternalOnlyAccount(user));
  }

  [Fact]
  public void ValidateCanUnlinkExternalLogin_WhenOnlyExternalMethod_ShouldFail()
  {
    var user = User.CreateInvited("external-only@example.com").Value;
    var login = ExternalLogin.Create(user.Id, "google", "subject-1").Value;
    user.AddExternalLogin(login);

    var result = ExternalAuthUtils.ValidateCanUnlinkExternalLogin(user, "google");

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void IsTwoFactorSetupRequired_WhenIdpMfaTrusted_ShouldReturnFalse()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    var auth = Options.Create(new AuthOptions
    {
      TwoFactor = new TwoFactorOptions
      {
        Enabled = true,
        Required = true,
        TrustIdentityProviderMfa = true
      }
    }).Value;

    Assert.False(ExternalAuthUtils.IsTwoFactorSetupRequired(user, auth, identityProviderMfaAsserted: true));
  }
}
