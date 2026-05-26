using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IPasskeyPolicyEvaluator
{
  bool IsPasskeysEnabledForDeployment();

  bool IsPasskeySetupRequired(User user, int passkeyCount);

  bool DoesPasskeySatisfyTwoFactor(bool userVerificationPresent);
}
