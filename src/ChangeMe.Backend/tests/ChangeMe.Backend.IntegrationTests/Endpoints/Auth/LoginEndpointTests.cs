using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class LoginEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenCredentialsAreValid_ShouldReturnSuccessStatusCode()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"login-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Login",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains("token", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains(email, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostLogin_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"login-invalid-{Guid.NewGuid():N}@example.com";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Login",
      LastName = "Invalid",
      Email = email,
      Password = "StrongPass123!"
    }, cancellationToken);

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = "WrongPass123!"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task PostLogin_WhenPasswordIsTooShort_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = "user@example.com",
      Password = "short"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
