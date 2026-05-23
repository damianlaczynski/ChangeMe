using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Domain.Interfaces;

public interface IPasswordExpirationEvaluator
{
  bool IsPasswordChangeRequired(User user, DateTime utcNow);

  DateTime? GetPasswordExpiresAtUtc(User user);
}
