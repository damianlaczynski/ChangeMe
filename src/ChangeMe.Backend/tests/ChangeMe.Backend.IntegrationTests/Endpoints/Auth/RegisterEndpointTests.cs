using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class RegisterEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostRegister_WhenRequestIsValid_ShouldCreateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var request = new
    {
      FirstName = "John",
      LastName = "Doe",
      Email = "john@example.com",
      Password = "StrongPass123!"
    };

    var response = await client.PostAsJsonAsync("/api/auth/register", request, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var registerResult = await IntegrationApiJson.ReadValueAsync<RegisterUserResponseDto>(
      response.Content,
      cancellationToken);

    Assert.NotNull(registerResult);
    Assert.False(registerResult!.RequiresEmailVerification);
    Assert.NotNull(registerResult.AuthSession);
    Assert.False(string.IsNullOrWhiteSpace(registerResult.AuthSession!.Token));

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = dbContext.Users.SingleOrDefault(x => x.Email == request.Email);

    Assert.NotNull(user);
    Assert.Equal(request.FirstName, user.FirstName);
    Assert.Equal(request.LastName, user.LastName);
    Assert.NotEmpty(user.PasswordHash);
  }

  [Fact]
  public async Task PostRegister_WhenPasswordMissingUppercase_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var request = new
    {
      FirstName = "Jane",
      LastName = "Doe",
      Email = $"jane-{Guid.NewGuid():N}@example.com",
      Password = "strongpass123!"
    };

    var response = await client.PostAsJsonAsync("/api/auth/register", request, cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("uppercase letter", responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostRegister_WhenPasswordIsTooShort_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var request = new
    {
      FirstName = "John",
      LastName = "Short",
      Email = "john-short@example.com",
      Password = "short"
    };

    var response = await client.PostAsJsonAsync("/api/auth/register", request, cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("Password", responseBody, StringComparison.OrdinalIgnoreCase);
  }
}
