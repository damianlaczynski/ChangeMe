using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using IdentityModel.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class OidcExternalAuthService(
  IHttpClientFactory httpClientFactory,
  IOptions<AuthOptions> authOptions) : IOidcExternalAuthService
{
  private static readonly string[] Scopes = ["openid", "profile", "email"];
  private readonly ConcurrentDictionary<string, string> _authorizationEndpoints = new(StringComparer.OrdinalIgnoreCase);

  public string BuildAuthorizationUrl(
    ExternalProviderConfiguration provider,
    ExternalAuthPending pending,
    string redirectUri)
  {
    var authorizationEndpoint = _authorizationEndpoints.GetOrAdd(
      provider.Authority.TrimEnd('/'),
      _ => ResolveAuthorizationEndpointFromDiscovery(provider.Authority));

    var query = new Dictionary<string, string?>(StringComparer.Ordinal)
    {
      ["client_id"] = provider.ClientId,
      ["response_type"] = "code",
      ["scope"] = string.Join(' ', Scopes),
      ["redirect_uri"] = redirectUri,
      ["state"] = pending.State,
      ["nonce"] = pending.Nonce,
      ["code_challenge"] = pending.CodeChallenge,
      ["code_challenge_method"] = "S256"
    };

    return QueryHelpers.AddQueryString(authorizationEndpoint, query);
  }

  private string ResolveAuthorizationEndpointFromDiscovery(string authority)
  {
    var configuration = GetConfigurationAsync(authority, CancellationToken.None).GetAwaiter().GetResult();
    if (string.IsNullOrWhiteSpace(configuration.AuthorizationEndpoint))
      throw new InvalidOperationException($"Authorization endpoint is not available for authority '{authority}'.");

    return configuration.AuthorizationEndpoint;
  }

  public async Task<Result<OidcSignInResult>> ExchangeAuthorizationCodeAsync(
    ExternalProviderConfiguration provider,
    ExternalAuthPending pending,
    string authorizationCode,
    string redirectUri,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(authorizationCode))
      return Result<OidcSignInResult>.Error("Authorization code is required.");

    var configuration = await GetConfigurationAsync(provider.Authority, cancellationToken);
    var httpClient = httpClientFactory.CreateClient(nameof(OidcExternalAuthService));

    var tokenResponse = await httpClient.RequestAuthorizationCodeTokenAsync(
      new AuthorizationCodeTokenRequest
      {
        Address = configuration.TokenEndpoint,
        ClientId = provider.ClientId,
        ClientSecret = provider.ClientSecret,
        Code = authorizationCode,
        RedirectUri = redirectUri,
        CodeVerifier = pending.CodeVerifier
      },
      cancellationToken);

    if (tokenResponse.IsError)
      return Result<OidcSignInResult>.Error(tokenResponse.Error ?? "Token exchange failed.");

    if (string.IsNullOrWhiteSpace(tokenResponse.IdentityToken))
      return Result<OidcSignInResult>.Error("Identity token is missing.");

    var principalResult = ValidateIdentityToken(
      tokenResponse.IdentityToken,
      configuration,
      provider.ClientId,
      provider.IssuerValidationMode,
      pending.Nonce);
    if (!principalResult.IsSuccess)
      return principalResult.Map();

    var principal = principalResult.Value;
    var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(subject))
      return Result<OidcSignInResult>.Error("Provider subject is missing.");

    var emailAssertion = OidcPrincipalClaimsReader.ReadEmail(
      principal,
      provider.TrustIdpEmailWithoutEmailVerified);
    var email = emailAssertion.Email;
    var emailVerified = emailAssertion.EmailVerified;
    var (firstName, lastName) = OidcPrincipalClaimsReader.ReadName(principal);

    return Result.Success(new OidcSignInResult(
      subject,
      email,
      emailVerified,
      firstName,
      lastName,
      IsIdentityProviderMfaAsserted(principal)));
  }

  private async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
    string authority,
    CancellationToken cancellationToken)
  {
    var normalizedAuthority = authority.TrimEnd('/');
    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
      $"{normalizedAuthority}/.well-known/openid-configuration",
      new OpenIdConnectConfigurationRetriever(),
      new HttpDocumentRetriever(httpClientFactory.CreateClient(nameof(OidcExternalAuthService)))
      {
        RequireHttps = authOptions.Value.FrontendBaseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase)
      });

    return await configurationManager.GetConfigurationAsync(cancellationToken);
  }

  private static Result<ClaimsPrincipal> ValidateIdentityToken(
    string identityToken,
    OpenIdConnectConfiguration configuration,
    string clientId,
    string issuerValidationMode,
    string expectedNonce)
  {
    var handler = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters
    {
      ValidAudience = clientId,
      IssuerSigningKeys = configuration.SigningKeys,
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ClockSkew = TimeSpan.FromMinutes(2)
    };

    OidcIssuerValidation.Configure(
      validationParameters,
      OidcIssuerValidationModeParser.Parse(issuerValidationMode),
      configuration);

    try
    {
      var principal = handler.ValidateToken(identityToken, validationParameters, out _);
      var nonce = principal.FindFirstValue(JwtRegisteredClaimNames.Nonce)
        ?? principal.FindFirstValue("nonce");
      if (!string.Equals(nonce, expectedNonce, StringComparison.Ordinal))
        return Result<ClaimsPrincipal>.Unauthorized();

      return Result.Success(principal);
    }
    catch (SecurityTokenException)
    {
      return Result<ClaimsPrincipal>.Unauthorized();
    }
  }

  private static bool IsIdentityProviderMfaAsserted(ClaimsPrincipal principal)
  {
    var amrValues = principal.FindAll("amr").Select(x => x.Value).ToList();
    return amrValues.Any(x => x.Equals("mfa", StringComparison.OrdinalIgnoreCase));
  }
}
