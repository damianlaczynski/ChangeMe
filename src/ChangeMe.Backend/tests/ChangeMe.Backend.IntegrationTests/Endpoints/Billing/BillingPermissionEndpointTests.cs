using System.Net;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class BillingPermissionEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetSettlementPeriods_WhenRegularUser_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/billing/settlement-periods", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetUserEmployment_WhenRegularUserViewsOtherUser_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var viewer = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var otherUser = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await viewer.Client.GetAsync(
      $"/api/users/{otherUser.UserId}/employment",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetTeamAvailabilityCalendar_WhenRegularUser_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync(
      "/api/billing/availability/calendar?From=2026-07-01&To=2026-07-31",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetMyAvailabilityCalendar_WhenRegularUser_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync(
      "/api/billing/my/availability/calendar?From=2026-07-01&To=2026-07-31",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}
