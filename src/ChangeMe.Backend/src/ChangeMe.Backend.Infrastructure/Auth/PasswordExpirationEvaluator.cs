using ChangeMe.Backend.Domain.Aggregates.Users;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class PasswordExpirationEvaluator(IOptions<AuthOptions> authOptions) : IPasswordExpirationEvaluator
{
  public bool IsPasswordChangeRequired(User user, DateTime utcNow)
  {
    var options = authOptions.Value;
    if (!options.PasswordExpirationEnabled || !user.HasPasswordSet)
      return false;

    if (user.PasswordLastChangedAt is null)
      return false;

    var ageDays = (utcNow - user.PasswordLastChangedAt.Value).TotalDays;
    return ageDays > options.MaximumPasswordAgeDays;
  }

  public DateTime? GetPasswordExpiresAtUtc(User user)
  {
    var options = authOptions.Value;
    if (!options.PasswordExpirationEnabled || !user.HasPasswordSet || user.PasswordLastChangedAt is null)
      return null;

    return user.PasswordLastChangedAt.Value.AddDays(options.MaximumPasswordAgeDays);
  }
}
