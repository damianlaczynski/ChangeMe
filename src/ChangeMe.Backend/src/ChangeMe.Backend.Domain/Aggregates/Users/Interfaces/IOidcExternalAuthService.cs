using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface IOidcExternalAuthService
{
  string BuildAuthorizationUrl(
    ExternalProviderConfiguration provider,
    ExternalAuthPending pending,
    string redirectUri);

  Task<Result<OidcSignInResult>> ExchangeAuthorizationCodeAsync(
    ExternalProviderConfiguration provider,
    ExternalAuthPending pending,
    string authorizationCode,
    string redirectUri,
    CancellationToken cancellationToken = default);
}

public sealed record ExternalProviderConfiguration(
  string ProviderKey,
  string DisplayName,
  string Authority,
  string ClientId,
  string ClientSecret,
  IReadOnlyList<string> AllowedEmailDomains,
  bool TrustIdpEmailWithoutEmailVerified,
  string IssuerValidationMode);

public sealed record OidcSignInResult(
  string ProviderSubject,
  string? Email,
  bool EmailVerified,
  string? FirstName,
  string? LastName,
  bool IdentityProviderMfaAsserted);
