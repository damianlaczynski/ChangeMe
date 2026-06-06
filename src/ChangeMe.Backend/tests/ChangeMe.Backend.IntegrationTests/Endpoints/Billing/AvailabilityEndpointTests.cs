using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Infrastructure.Email;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class AvailabilityEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task CreateManualEntry_WhenValid_ShouldPersistEntry()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.PostAsJsonAsync("/api/billing/my/availability/entries", new
    {
      StartDate = "2026-07-01",
      EndDate = "2026-07-01",
      AllDay = true,
      StartTime = (string?)null,
      EndTime = (string?)null,
      Status = "Remote",
      Notes = "Working remotely",
    }, cancellationToken);

    response.EnsureSuccessStatusCode();

    var calendarResponse = await user.Client.GetAsync(
      "/api/billing/my/availability/calendar?From=2026-07-01&To=2026-07-31",
      cancellationToken);
    calendarResponse.EnsureSuccessStatusCode();

    var body = await calendarResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Remote", body, StringComparison.Ordinal);
    Assert.Contains("Manual", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task CreateManualEntry_WhenOverlapping_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var firstResponse = await user.Client.PostAsJsonAsync("/api/billing/my/availability/entries", new
    {
      StartDate = "2026-07-10",
      EndDate = "2026-07-10",
      AllDay = true,
      Status = "Available",
      Notes = (string?)null,
    }, cancellationToken);
    firstResponse.EnsureSuccessStatusCode();

    var secondResponse = await user.Client.PostAsJsonAsync("/api/billing/my/availability/entries", new
    {
      StartDate = "2026-07-10",
      EndDate = "2026-07-10",
      AllDay = true,
      Status = "Unavailable",
      Notes = (string?)null,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    var body = await secondResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("overlap", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task ApproveLeaveRequest_ShouldCreateLeaveAvailabilityEntries()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var leaveTypeId = await GetVacationLeaveTypeIdAsync(admin.Client, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-requests", new
    {
      UserId = employee.UserId,
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-08-11",
      EndDate = "2026-08-12",
      DayPortion = (string?)null,
      Reason = "Vacation",
      Submit = false,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var requestId = await ExtractRequestIdAsync(createResponse, cancellationToken);

    await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/submit",
      new { },
      cancellationToken);

    await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/approve",
      new { },
      cancellationToken);

    var calendarResponse = await employee.Client.GetAsync(
      "/api/billing/my/availability/calendar?From=2026-08-01&To=2026-08-31",
      cancellationToken);
    calendarResponse.EnsureSuccessStatusCode();

    var body = await calendarResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Leave", body, StringComparison.Ordinal);
    Assert.Contains("Unavailable", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task CreateManualEntryForAnotherUser_WhenAdministrator_ShouldNotifyAffectedUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
      fakeEmail?.Clear();
    }

    var response = await admin.Client.PostAsJsonAsync("/api/billing/availability/entries", new
    {
      UserId = employee.UserId,
      StartDate = "2026-09-15",
      EndDate = "2026-09-15",
      AllDay = true,
      Status = "Remote",
      Notes = (string?)null,
    }, cancellationToken);
    response.EnsureSuccessStatusCode();

    var notificationsResponse = await employee.Client.GetAsync("/api/notifications", cancellationToken);
    notificationsResponse.EnsureSuccessStatusCode();

    var body = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Your availability was updated", body, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("AVAILABILITY_UPDATED_BY_ADMIN", body, StringComparison.OrdinalIgnoreCase);

    await using var assertScope = factory.Services.CreateAsyncScope();
    var emailService = (FakeEmailService)assertScope.ServiceProvider.GetRequiredService<IEmailService>();
    Assert.Contains(
      emailService.SentEmails,
      message => message.Subject.Contains("Your availability was updated", StringComparison.OrdinalIgnoreCase)
                   && message.Recipients.Contains(employee.Email));
  }

  [Fact]
  public async Task CreateManualEntry_WhenSelfService_ShouldCreateInAppNotification()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.PostAsJsonAsync("/api/billing/my/availability/entries", new
    {
      StartDate = "2026-09-20",
      EndDate = "2026-09-20",
      AllDay = true,
      Status = "Available",
      Notes = (string?)null,
    }, cancellationToken);
    response.EnsureSuccessStatusCode();

    var notificationsResponse = await user.Client.GetAsync("/api/notifications", cancellationToken);
    notificationsResponse.EnsureSuccessStatusCode();

    var body = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Your availability was updated for", body, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("AVAILABILITY_UPDATED_BY_SELF", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task SaveWeeklyPattern_WhenUpdated_ShouldRegenerateRecurringEntries()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var saveResponse = await user.Client.PutAsJsonAsync("/api/billing/my/availability/pattern", new
    {
      Days = BuildSingleEnabledDayPattern(DayOfWeek.Tuesday, "10:00", "14:00", "Remote"),
    }, cancellationToken);
    saveResponse.EnsureSuccessStatusCode();

    var calendarResponse = await user.Client.GetAsync(
      "/api/billing/my/availability/calendar?From=2026-10-01&To=2026-10-31",
      cancellationToken);
    calendarResponse.EnsureSuccessStatusCode();

    var body = await calendarResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var recurringEntries = document.RootElement
      .GetProperty("value")
      .GetProperty("entries")
      .EnumerateArray()
      .Where(entry => entry.GetProperty("source").GetString() == "Recurring")
      .ToList();

    Assert.NotEmpty(recurringEntries);
    Assert.All(recurringEntries, entry =>
    {
      Assert.Equal("Remote", entry.GetProperty("status").GetString());
      Assert.Equal("10:00:00", entry.GetProperty("startTime").GetString());
      Assert.Equal("14:00:00", entry.GetProperty("endTime").GetString());
    });
  }

  [Fact]
  public async Task ResetUserWeeklyPattern_WhenAdministrator_ShouldNotifyAndEmailEmployee()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await employee.Client.PutAsJsonAsync("/api/billing/my/availability/pattern", new
    {
      Days = BuildSingleEnabledDayPattern(DayOfWeek.Wednesday, "09:00", "17:00", "OnSite"),
    }, cancellationToken);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
      fakeEmail?.Clear();
    }

    var resetResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/users/{employee.UserId}/availability/pattern/reset",
      new { },
      cancellationToken);
    resetResponse.EnsureSuccessStatusCode();

    var notificationsResponse = await employee.Client.GetAsync("/api/notifications", cancellationToken);
    notificationsResponse.EnsureSuccessStatusCode();

    var notificationsBody = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("weekly availability pattern was updated", notificationsBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("WEEKLY_PATTERN_UPDATED_BY_ADMIN", notificationsBody, StringComparison.OrdinalIgnoreCase);

    await using var assertScope = factory.Services.CreateAsyncScope();
    var emailService = (FakeEmailService)assertScope.ServiceProvider.GetRequiredService<IEmailService>();
    Assert.Contains(
      emailService.SentEmails,
      message => message.Subject.Contains("weekly availability pattern", StringComparison.OrdinalIgnoreCase)
                   && message.Recipients.Contains(employee.Email));
  }

  [Fact]
  public async Task ApproveHalfDayLeaveRequest_ShouldCreateTimedLeaveAvailabilityEntry()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var leaveTypeId = await GetVacationLeaveTypeIdAsync(admin.Client, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/leave-requests", new
    {
      UserId = employee.UserId,
      LeaveTypeId = leaveTypeId,
      StartDate = "2026-08-20",
      EndDate = "2026-08-20",
      DayPortion = "FirstHalf",
      Reason = "Morning appointment",
      Submit = false,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var requestId = await ExtractRequestIdAsync(createResponse, cancellationToken);

    await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/submit",
      new { },
      cancellationToken);

    await admin.Client.PostAsJsonAsync(
      $"/api/billing/leave-requests/{requestId}/approve",
      new { },
      cancellationToken);

    var calendarResponse = await employee.Client.GetAsync(
      "/api/billing/my/availability/calendar?From=2026-08-20&To=2026-08-20",
      cancellationToken);
    calendarResponse.EnsureSuccessStatusCode();

    var settingsResponse = await admin.Client.GetAsync("/api/billing/settings", cancellationToken);
    settingsResponse.EnsureSuccessStatusCode();
    var settingsBody = await settingsResponse.Content.ReadAsStringAsync(cancellationToken);
    using var settingsDocument = JsonDocument.Parse(settingsBody);
    var settings = settingsDocument.RootElement.GetProperty("value");
    var expectedStart = settings.GetProperty("defaultWorkdayStart").GetString();
    var expectedEnd = settings.GetProperty("halfDaySplitTime").GetString();

    var body = await calendarResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var leaveEntry = document.RootElement
      .GetProperty("value")
      .GetProperty("entries")
      .EnumerateArray()
      .Single(entry => entry.GetProperty("source").GetString() == "Leave");

    Assert.False(leaveEntry.GetProperty("allDay").GetBoolean());
    Assert.Equal($"{expectedStart}:00", leaveEntry.GetProperty("startTime").GetString());
    Assert.Equal($"{expectedEnd}:00", leaveEntry.GetProperty("endTime").GetString());
    Assert.Equal("Unavailable", leaveEntry.GetProperty("status").GetString());
  }

  [Fact]
  public async Task DeleteManualEntryForAnotherUser_WhenAdministrator_ShouldNotifyAndEmailEmployee()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var createResponse = await employee.Client.PostAsJsonAsync("/api/billing/my/availability/entries", new
    {
      StartDate = "2026-09-16",
      EndDate = "2026-09-16",
      AllDay = true,
      Status = "Remote",
      Notes = (string?)null,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var entryId = await ExtractEntryIdAsync(createResponse, cancellationToken);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
      fakeEmail?.Clear();
    }

    var deleteResponse = await admin.Client.DeleteAsync(
      $"/api/billing/availability/entries/{entryId}",
      cancellationToken);
    deleteResponse.EnsureSuccessStatusCode();

    var notificationsResponse = await employee.Client.GetAsync("/api/notifications", cancellationToken);
    notificationsResponse.EnsureSuccessStatusCode();

    var notificationsBody = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Your availability was updated", notificationsBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("AVAILABILITY_UPDATED_BY_ADMIN", notificationsBody, StringComparison.OrdinalIgnoreCase);

    await using var assertScope = factory.Services.CreateAsyncScope();
    var emailService = (FakeEmailService)assertScope.ServiceProvider.GetRequiredService<IEmailService>();
    Assert.Contains(
      emailService.SentEmails,
      message => message.Subject.Contains("Your availability was updated", StringComparison.OrdinalIgnoreCase)
                   && message.Recipients.Contains(employee.Email));
  }

  [Fact]
  public async Task SaveWeeklyPattern_WhenSelfService_ShouldCreateInAppNotification()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var saveResponse = await user.Client.PutAsJsonAsync("/api/billing/my/availability/pattern", new
    {
      Days = BuildSingleEnabledDayPattern(DayOfWeek.Thursday, "09:00", "17:00", "OnSite"),
    }, cancellationToken);
    saveResponse.EnsureSuccessStatusCode();

    var notificationsResponse = await user.Client.GetAsync("/api/notifications", cancellationToken);
    notificationsResponse.EnsureSuccessStatusCode();

    var body = await notificationsResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("weekly pattern", body, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("AVAILABILITY_UPDATED_BY_SELF", body, StringComparison.OrdinalIgnoreCase);
  }

  private static object[] BuildSingleEnabledDayPattern(
    DayOfWeek enabledDay,
    string startTime,
    string endTime,
    string status)
  {
    return Enum.GetValues<DayOfWeek>()
      .Select(day => new
      {
        DayOfWeek = day,
        Enabled = day == enabledDay,
        StartTime = day == enabledDay ? startTime : (string?)null,
        EndTime = day == enabledDay ? endTime : (string?)null,
        Status = day == enabledDay ? status : (string?)null,
      })
      .ToArray();
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

  private static async Task<Guid> ExtractRequestIdAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    return document.RootElement.GetProperty("value").GetProperty("id").GetGuid();
  }

  private static async Task<Guid> ExtractEntryIdAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    return document.RootElement.GetProperty("value").GetProperty("id").GetGuid();
  }
}
