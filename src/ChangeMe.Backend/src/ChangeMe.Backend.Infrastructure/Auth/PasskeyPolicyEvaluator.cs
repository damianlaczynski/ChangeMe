using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class PasskeyPolicyEvaluator(IOptions<AuthOptions> authOptions) : IPasskeyPolicyEvaluator
{
  public bool IsPasskeysEnabledForDeployment() =>
    authOptions.Value.Passkeys.PasskeysAuthenticationEnabled;

  public bool IsPasskeySetupRequired(User user, int passkeyCount)
  {
    var options = authOptions.Value.Passkeys;
    if (!options.PasskeysAuthenticationEnabled || !options.PasskeysAuthenticationRequired)
      return false;

    if (!user.IsActive)
      return false;

    return passkeyCount == 0;
  }

  public bool DoesPasskeySatisfyTwoFactor(bool userVerificationPresent)
  {
    var options = authOptions.Value;
    return options.Passkeys.PasskeySatisfiesTwoFactor
      && options.Passkeys.PasskeysAuthenticationEnabled
      && options.TwoFactorAuthenticationEnabled
      && userVerificationPresent;
  }
}
