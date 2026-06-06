using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class LeaveTypesAndSettingsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetLeaveTypes_WhenAdministrator_ShouldReturnSeededTypes()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.GetAsync("/api/billing/leave-types", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Vacation", body, StringComparison.Ordinal);
    Assert.Contains("VAC", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostLeaveType_WhenAdministrator_ShouldCreateLeaveType()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var name = $"Leave-{Guid.NewGuid():N}"[..20];
    var code = $"L{Guid.NewGuid():N}"[..4].ToUpperInvariant();

    var response = await admin.Client.PostAsJsonAsync("/api/billing/leave-types", new
    {
      Name = name,
      Code = code,
      CountsAsPaid = true,
      UsesAllowance = false,
      RequiresApproval = true,
      IsActive = true,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(name, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PutBillingSettings_WhenAdministrator_ShouldUpdateSettings()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.PutAsJsonAsync("/api/billing/settings", new
    {
      DefaultAnnualLeaveDays = 25.5m,
      AllowHalfDayLeave = true,
      DefaultWorkdayStart = "08:30",
      DefaultWorkdayEnd = "16:30",
      HalfDaySplitTime = "12:30",
      DefaultWorkdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
      DefaultAvailabilityStatus = "Remote",
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var getResponse = await admin.Client.GetAsync("/api/billing/settings", cancellationToken);
    getResponse.EnsureSuccessStatusCode();

    var body = await getResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var value = document.RootElement.GetProperty("value");
    Assert.Equal(25.5m, value.GetProperty("defaultAnnualLeaveDays").GetDecimal());
    Assert.Equal("Remote", value.GetProperty("defaultAvailabilityStatus").GetString());
  }

  [Fact]
  public async Task DeleteLeaveType_WhenSeededType_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var leaveTypeId = await GetVacationLeaveTypeIdAsync(admin.Client, cancellationToken);

    var response = await admin.Client.DeleteAsync(
      $"/api/billing/leave-types/{leaveTypeId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(BillingConstraints.SeededLeaveTypeDeleteMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task DeleteLeaveType_WhenReferencedByLeaveRequest_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var name = $"Leave-{Guid.NewGuid():N}"[..20];
    var code = $"L{Guid.NewGuid():N}"[..4].ToUpperInvariant();

    var createTypeResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-types", new
    {
      Name = name,
      Code = code,
      CountsAsPaid = true,
      UsesAllowance = false,
      RequiresApproval = true,
      IsActive = true,
    }, cancellationToken);
    createTypeResponse.EnsureSuccessStatusCode();

    var createTypeBody = await createTypeResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createTypeDocument = JsonDocument.Parse(createTypeBody);
    var leaveTypeId = createTypeDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var createRequestResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-requests", new
    {
      UserId = employee.UserId,
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-09-01",
      EndDate = "2026-09-01",
      DayPortion = (string?)null,
      Reason = "Referenced leave type test",
      Submit = false,
    }, cancellationToken);
    createRequestResponse.EnsureSuccessStatusCode();

    var deleteResponse = await admin.Client.DeleteAsync(
      $"/api/billing/leave-types/{leaveTypeId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

    var body = await deleteResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(BillingConstraints.LeaveTypeReferencedMessage, body, StringComparison.Ordinal);
  }

  private static async Task<Guid> GetVacationLeaveTypeIdAsync(
    HttpClient client,
    CancellationToken cancellationToken)
  {
    var response = await client.GetAsync("/api/billing/leave-types", cancellationToken);
    response.EnsureSuccessStatusCode();

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    foreach (var item in document.RootElement.GetProperty("value").EnumerateArray())
    {
      if (item.GetProperty("code").GetString() == "VAC")
        return item.GetProperty("id").GetGuid();
    }

    throw new InvalidOperationException("Vacation leave type was not seeded.");
  }
}
