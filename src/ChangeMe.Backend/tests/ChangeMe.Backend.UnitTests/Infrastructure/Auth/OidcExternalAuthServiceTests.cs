using System.Net;
using System.Text;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class OidcExternalAuthServiceTests
{
  [Fact]
  public void BuildAuthorizationUrl_ForGoogle_ShouldUseDiscoveredAuthorizationEndpoint()
  {
    var service = CreateService(
      ("https://accounts.google.com", "https://accounts.google.com/o/oauth2/v2/auth"));

    var url = BuildUrl(service, "google", "https://accounts.google.com");

    Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth?", url, StringComparison.Ordinal);
    Assert.Contains("response_type=code", url, StringComparison.Ordinal);
    Assert.Contains("client_id=client-id", url, StringComparison.Ordinal);
    Assert.Contains("code_challenge=challenge", url, StringComparison.Ordinal);
    Assert.Contains("code_challenge_method=S256", url, StringComparison.Ordinal);
  }

  [Fact]
  public void BuildAuthorizationUrl_ForRealmStyleAuthority_ShouldUseDiscoveredAuthorizationEndpoint()
  {
    var service = CreateService(
      ("http://localhost:8080/realms/example",
        "http://localhost:8080/realms/example/protocol/openid-connect/auth"));

    var url = BuildUrl(
      service,
      "oidc-realm",
      "http://localhost:8080/realms/example",
      clientId: "example-web");

    Assert.StartsWith(
      "http://localhost:8080/realms/example/protocol/openid-connect/auth?",
      url,
      StringComparison.Ordinal);
    Assert.Contains("client_id=example-web", url, StringComparison.Ordinal);
  }

  [Fact]
  public void BuildAuthorizationUrl_ForMicrosoft_ShouldUseDiscoveredAuthorizationEndpoint()
  {
    var service = CreateService(
      ("https://login.microsoftonline.com/common/v2.0",
        "https://login.microsoftonline.com/common/oauth2/v2.0/authorize"));

    var url = BuildUrl(service, "microsoft", "https://login.microsoftonline.com/common/v2.0");

    Assert.StartsWith(
      "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?",
      url,
      StringComparison.Ordinal);
  }

  private static OidcExternalAuthService CreateService(params (string Authority, string AuthorizationEndpoint)[] endpoints)
  {
    return new OidcExternalAuthService(
      new DiscoveryStubHttpClientFactory(endpoints),
      Options.Create(new AuthOptions()));
  }

  private static string BuildUrl(
    OidcExternalAuthService service,
    string providerKey,
    string authority,
    string clientId = "client-id",
    string clientSecret = "client-secret")
  {
    var pending = ExternalAuthPending.CreateSignIn(
      providerKey,
      state: "state-value",
      nonce: "nonce-value",
      codeChallenge: "challenge",
      codeVerifier: "verifier",
      expiresAtUtc: DateTime.UtcNow.AddMinutes(10)).Value;

    var provider = new ExternalProviderConfiguration(
      providerKey,
      providerKey,
      authority,
      clientId,
      clientSecret,
      [],
      TrustIdpEmailWithoutEmailVerified: false,
      IssuerValidationMode: "Discovery");

    return service.BuildAuthorizationUrl(
      provider,
      pending,
      "http://localhost:4200/external-sign-in/callback");
  }

  private sealed class DiscoveryStubHttpClientFactory(
    (string Authority, string AuthorizationEndpoint)[] endpoints) : IHttpClientFactory
  {
    public HttpClient CreateClient(string name) => new(new DiscoveryHandler(endpoints));
  }

  private sealed class DiscoveryHandler((string Authority, string AuthorizationEndpoint)[] endpoints)
    : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      var requestUri = request.RequestUri?.ToString() ?? string.Empty;

      if (requestUri.Contains("/jwks", StringComparison.Ordinal)
          || requestUri.EndsWith("/keys", StringComparison.Ordinal))
      {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
          Content = new StringContent("{\"keys\":[]}", Encoding.UTF8, "application/json")
        });
      }

      if (!requestUri.Contains(".well-known/openid-configuration", StringComparison.Ordinal))
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

      var match = endpoints.FirstOrDefault(x =>
        requestUri.StartsWith(x.Authority.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
      if (match.Authority is null)
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

      var document = new
      {
        issuer = match.Authority,
        authorization_endpoint = match.AuthorizationEndpoint,
        token_endpoint = $"{match.Authority.TrimEnd('/')}/token",
        jwks_uri = $"{match.Authority.TrimEnd('/')}/jwks"
      };

      return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(
          JsonSerializer.Serialize(document),
          Encoding.UTF8,
          "application/json")
      });
    }
  }
}
