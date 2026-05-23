using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(PublicRegistrationIntegrationTestCollection.Name)]
public sealed class PublicRegistrationEndpointTests(PublicRegistrationDisabledWebApplicationFactory factory)
{
  [Fact]
  public async Task PostRegister_WhenPublicRegistrationDisabled_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Blocked",
      LastName = "Register",
      Email = $"blocked-{Guid.NewGuid():N}@example.com",
      Password = "StrongPass123!"
    }, cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    Assert.Contains(AuthSessionUtils.RegistrationDisabledMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}
