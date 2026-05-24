namespace ChangeMe.Backend.Domain.Aggregates.Sessions;

public static class SignInMethods
{
  public const string Password = "Password";
  public const string Passkey = "Passkey";

  public const string Registration = "Registration";

  public const string ExternalProviderPrefix = "External:";

  /// <summary>
  /// Canonical value persisted on sessions and pending 2FA challenges for OIDC sign-in.
  /// The host resolves a display name from configured external providers when listing sessions.
  /// </summary>
  public static string ExternalWithProvider(string providerKey)
  {
    if (string.IsNullOrWhiteSpace(providerKey))
      throw new ArgumentException("Provider key is required.", nameof(providerKey));

    var trimmed = providerKey.Trim();
    return $"{ExternalProviderPrefix}{trimmed}";
  }
}
