using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class LeaveRequestsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task CreateSubmitAndApproveLeaveRequest_WhenAdministrator_ShouldCompleteWorkflow()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var leaveTypeId = await GetVacationLeaveTypeIdAsync(admin.Client, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-requests", new
    {
      UserId = employee.UserId,
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-08-04",
      EndDate = "2026-08-06",
      DayPortion = (string?)null,
      Reason = "Summer break",
      Submit = false,
    }, cancellationToken);

    Assert.True(createResponse.IsSuccessStatusCode);

    var requestId = await ExtractRequestIdAsync(createResponse, cancellationToken);
    Assert.NotEqual(Guid.Empty, requestId);

    var submitResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/submit",
      new { },
      cancellationToken);
    submitResponse.EnsureSuccessStatusCode();

    var approveResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/approve",
      new { },
      cancellationToken);
    approveResponse.EnsureSuccessStatusCode();

    var detailsResponse = await admin.Client.GetAsync(
      $"/api/billing/leave-requests/{requestId}",
      cancellationToken);
    detailsResponse.EnsureSuccessStatusCode();

    var body = await detailsResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var value = document.RootElement.GetProperty("value");
    Assert.Equal("Approved", value.GetProperty("status").GetString());
    Assert.Equal(3m, value.GetProperty("days").GetDecimal());
  }

  [Fact]
  public async Task ApproveLeaveRequest_WhenApproverIsRequestOwner_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var leaveTypeId = await GetVacationLeaveTypeIdAsync(admin.Client, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/my/leave-requests", new
    {
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-09-01",
      EndDate = "2026-09-01",
      DayPortion = "FullDay",
      Reason = "Personal day",
      Submit = true,
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();
    var requestId = await ExtractRequestIdAsync(createResponse, cancellationToken);

    var approveResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/approve",
      new { },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, approveResponse.StatusCode);

    var body = await approveResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("cannot approve your own leave request", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task CreateLeaveRequest_WhenDatesOverlapSubmittedRequest_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var leaveTypeId = await GetVacationLeaveTypeIdAsync(admin.Client, cancellationToken);

    var firstResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-requests", new
    {
      UserId = employee.UserId,
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-10-01",
      EndDate = "2026-10-03",
      DayPortion = (string?)null,
      Reason = "First request",
      Submit = true,
    }, cancellationToken);
    firstResponse.EnsureSuccessStatusCode();

    var secondResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-requests", new
    {
      UserId = employee.UserId,
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-10-02",
      EndDate = "2026-10-04",
      DayPortion = (string?)null,
      Reason = "Overlapping request",
      Submit = true,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    var body = await secondResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("overlap", body, StringComparison.OrdinalIgnoreCase);
  }

  private static async Task<Guid> GetVacationLeaveTypeIdAsync(
    HttpClient client,
    CancellationToken cancellationToken)
  {
    var response = await client.GetAsync("/api/billing/leave-types", cancellationToken);
    response.EnsureSuccessStatusCode();

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var items = document.RootElement.GetProperty("value");

    foreach (var item in items.EnumerateArray())
    {
      if (item.GetProperty("code").GetString() == "VAC")
        return item.GetProperty("id").GetGuid();
    }

    throw new InvalidOperationException("Vacation leave type was not seeded.");
  }

  private static async Task<Guid> ExtractRequestIdAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    return document.RootElement.GetProperty("value").GetProperty("id").GetGuid();
  }
}
