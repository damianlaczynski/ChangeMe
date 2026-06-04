using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(EmailChangeDisabledIntegrationTestCollection.Name)]
public sealed class EmailChangeDisabledEndpointTests(EmailChangeDisabledWebApplicationFactory factory)
{
  [Fact]
  public async Task PostEmailChange_WhenFeatureDisabled_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string password = "StrongPass123!";
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var newEmail = $"new-{Guid.NewGuid():N}@example.com";

    var response = await user.Client.PostAsJsonAsync("/api/auth/email-change", new
    {
      NewEmail = newEmail,
      CurrentPassword = password,
      VerificationCode = (string?)null
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(
      RequestEmailChangeHandler.EmailChangeDisabledMessage,
      body,
      StringComparison.OrdinalIgnoreCase);
  }
}
