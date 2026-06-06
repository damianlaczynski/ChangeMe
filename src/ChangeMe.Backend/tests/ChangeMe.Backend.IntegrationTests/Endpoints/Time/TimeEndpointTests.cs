using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Time;

[Collection(IntegrationTestCollection.Name)]
public sealed class TimeEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostTimeEntry_WhenAuthorized_ShouldCreateEntry()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);

    var response = await user.Client.PostAsJsonAsync("/api/time/entries", new
    {
      ProjectId = projectId,
      IssueId = (Guid?)null,
      WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
      DurationMinutes = 45,
      Description = "Implemented feature"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("45m", body, StringComparison.Ordinal);
    Assert.Contains("Implemented feature", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostTimeEntry_WhenWorkDateOutsideBackdatingLimit_ShouldReturnInvalid()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);
    var workDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(TimeConstraints.DefaultBackdatingLimitDays + 1)));

    var response = await user.Client.PostAsJsonAsync("/api/time/entries", new
    {
      ProjectId = projectId,
      IssueId = (Guid?)null,
      WorkDate = workDate,
      DurationMinutes = 30,
      Description = "Old work"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(TimeConstraints.WorkDateOutsideRangeMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task RunningTimer_StartAndDiscard_ShouldSucceed()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);

    var startResponse = await user.Client.PostAsJsonAsync("/api/time/running-timer", new
    {
      ProjectId = projectId,
      IssueId = (Guid?)null,
      ReplaceExisting = false,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

    var getResponse = await user.Client.GetAsync("/api/time/running-timer", cancellationToken);
    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

    var runningBody = await getResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("timer", runningBody, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("\"timer\":null", runningBody, StringComparison.OrdinalIgnoreCase);

    var discardResponse = await user.Client.DeleteAsync("/api/time/running-timer", cancellationToken);
    Assert.Equal(HttpStatusCode.OK, discardResponse.StatusCode);

    var afterDiscardResponse = await user.Client.GetAsync("/api/time/running-timer", cancellationToken);
    afterDiscardResponse.EnsureSuccessStatusCode();
    var afterDiscardBody = await afterDiscardResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("\"timer\":null", afterDiscardBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetMyTimeEntries_WhenFilteredByProject_ShouldReturnMatchingEntries()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var defaultProjectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);
    var otherProjectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);
    var marker = $"filter-{Guid.NewGuid():N}";

    await TimeTestHelper.CreateTimeEntryAsync(
      user.Client,
      defaultProjectId,
      cancellationToken,
      description: marker);

    await TimeTestHelper.CreateTimeEntryAsync(
      user.Client,
      otherProjectId,
      cancellationToken,
      description: $"{marker}-other");

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var monthStart = new DateOnly(today.Year, today.Month, 1);

    var response = await user.Client.GetAsync(
      $"/api/time/my-entries?projectId={defaultProjectId}&dateFrom={monthStart:yyyy-MM-dd}&dateTo={today:yyyy-MM-dd}&pageNumber=1&pageSize=20",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(marker, body, StringComparison.Ordinal);
    Assert.DoesNotContain($"{marker}-other", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetReportPersonEntries_WhenAdministratorViewsAnotherUser_ShouldReturnEntries()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var otherUser = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);
    var marker = $"report-drilldown-{Guid.NewGuid():N}";
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    await TimeTestHelper.CreateTimeEntryAsync(
      otherUser.Client,
      projectId,
      cancellationToken,
      description: marker,
      workDate: today,
      durationMinutes: 90);

    var monthStart = new DateOnly(today.Year, today.Month, 1);
    var response = await admin.Client.GetAsync(
      $"/api/time/reports/person-entries?userId={otherUser.UserId}&dateFrom={monthStart:yyyy-MM-dd}&dateTo={today:yyyy-MM-dd}&pageNumber=1&pageSize=20",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(marker, body, StringComparison.Ordinal);
    Assert.Contains("1h 30m", body, StringComparison.Ordinal);
  }
}
