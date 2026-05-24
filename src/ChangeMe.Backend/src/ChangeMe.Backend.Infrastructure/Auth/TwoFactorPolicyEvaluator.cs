using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class TwoFactorPolicyEvaluator(IOptions<AuthOptions> authOptions) : ITwoFactorPolicyEvaluator
{
  public bool IsTwoFactorEnabledForDeployment() => authOptions.Value.TwoFactorAuthenticationEnabled;

  public bool IsTwoFactorVerificationRequired(User user) =>
    IsTwoFactorEnabledForDeployment() && user.TwoFactorEnabled;

  public bool IsTwoFactorSetupRequired(User user)
  {
    var options = authOptions.Value;
    if (!options.TwoFactorAuthenticationEnabled || !options.TwoFactorAuthenticationRequired)
      return false;

    if (!user.HasPasswordSet)
      return false;

    return !user.TwoFactorEnabled;
  }
}
