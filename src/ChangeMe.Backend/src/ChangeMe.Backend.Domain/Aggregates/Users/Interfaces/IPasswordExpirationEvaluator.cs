namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IPasswordExpirationEvaluator
{
  bool IsPasswordChangeRequired(User user, DateTime utcNow);

  DateTime? GetPasswordExpiresAtUtc(User user);
}
