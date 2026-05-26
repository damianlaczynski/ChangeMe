namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface ITwoFactorPolicyEvaluator
{
  bool IsTwoFactorSetupRequired(User user);

  bool IsTwoFactorEnabledForDeployment();

  bool IsTwoFactorVerificationRequired(User user);
}
