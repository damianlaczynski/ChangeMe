using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class SignInMethodDisplay
{
  public static string Format(string signInMethod, AuthOptions auth)
  {
    if (string.IsNullOrWhiteSpace(signInMethod))
      return "Unknown";

    if (signInMethod == SignInMethods.Password)
      return "Password";

    if (signInMethod == SignInMethods.Passkey)
      return "Passkey";

    if (signInMethod == SignInMethods.Registration)
      return "Registration";

    if (signInMethod.StartsWith(SignInMethods.ExternalProviderPrefix, StringComparison.OrdinalIgnoreCase))
    {
      var key = signInMethod[SignInMethods.ExternalProviderPrefix.Length..];
      if (string.IsNullOrWhiteSpace(key))
        return "Unknown";

      var provider = auth.External.Providers
        .FirstOrDefault(x => x.ProviderKey.Equals(key, StringComparison.OrdinalIgnoreCase));
      return string.IsNullOrWhiteSpace(provider?.DisplayName) ? key : provider.DisplayName;
    }

    return signInMethod;
  }
}
