using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

namespace ChangeMe.Backend.IntegrationTests.Support.Fakes;

public sealed class FakeOidcExternalAuthService : IOidcExternalAuthService
{
  public const string ProviderKey = "test";
  public const string InvalidCode = "invalid";

  public string BuildAuthorizationUrl(
    ExternalProviderConfiguration provider,
    ExternalAuthPending pending,
    string redirectUri) =>
    $"https://fake-idp.test/authorize?state={Uri.EscapeDataString(pending.State)}";

  public Task<Result<OidcSignInResult>> ExchangeAuthorizationCodeAsync(
    ExternalProviderConfiguration provider,
    ExternalAuthPending pending,
    string authorizationCode,
    string redirectUri,
    CancellationToken cancellationToken = default)
  {
    if (authorizationCode == InvalidCode)
      return Task.FromResult(Result<OidcSignInResult>.Unauthorized());

    var emailVerified = false;
    string? email = null;
    var identityProviderMfaAsserted = false;
    string? providerSubject = null;

    if (authorizationCode.StartsWith("subject:", StringComparison.Ordinal))
    {
      providerSubject = authorizationCode["subject:".Length..];
      email = "stepup@example.com";
      emailVerified = true;
    }
    else if (authorizationCode.StartsWith("email:", StringComparison.Ordinal))
    {
      email = authorizationCode["email:".Length..];
      emailVerified = true;
    }
    else if (authorizationCode.StartsWith("unverified:", StringComparison.Ordinal))
    {
      email = authorizationCode["unverified:".Length..];
    }
    else if (authorizationCode == "mfa")
    {
      email = $"mfa-{Guid.NewGuid():N}@example.com";
      emailVerified = true;
      identityProviderMfaAsserted = true;
    }

    return Task.FromResult(Result.Success(new OidcSignInResult(
      providerSubject ?? $"subject-{pending.State}",
      email,
      emailVerified,
      "Oidc",
      "User",
      identityProviderMfaAsserted)));
  }
}
