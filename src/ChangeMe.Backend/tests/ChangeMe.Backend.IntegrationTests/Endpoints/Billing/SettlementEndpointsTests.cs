using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class SettlementEndpointsTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task CreateSettlementPeriod_WhenAdministrator_ShouldCreateAndCalculate()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var year = 2026;
    var month = 11;

    await SeedEmploymentWithContractAsync(admin, employee, year, month, cancellationToken);

    var response = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = year,
      Month = month,
    }, cancellationToken);

    Assert.True(response.IsSuccessStatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var value = document.RootElement.GetProperty("value");
    Assert.Equal("Open", value.GetProperty("status").GetString());
    Assert.True(value.GetProperty("userSettlements").GetArrayLength() >= 1);
  }

  [Fact]
  public async Task CloseSettlementPeriod_WhenOpen_ShouldClosePeriod()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var year = 2026;
    var month = 10;

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = year,
      Month = month,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createDocument = JsonDocument.Parse(createBody);
    var periodId = createDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var closeResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/settlement-periods/{periodId}/close",
      new { },
      cancellationToken);
    closeResponse.EnsureSuccessStatusCode();

    var closeBody = await closeResponse.Content.ReadAsStringAsync(cancellationToken);
    using var closeDocument = JsonDocument.Parse(closeBody);
    Assert.Equal("Closed", closeDocument.RootElement.GetProperty("value").GetProperty("status").GetString());
  }

  [Fact]
  public async Task RecalculateUserSettlement_WhenPeriodClosed_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var year = 2026;
    var month = 9;

    await SeedEmploymentWithContractAsync(admin, employee, year, month, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = year,
      Month = month,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createDocument = JsonDocument.Parse(createBody);
    var periodValue = createDocument.RootElement.GetProperty("value");
    var periodId = periodValue.GetProperty("id").GetGuid();
    var settlementId = periodValue.GetProperty("userSettlements")[0].GetProperty("id").GetGuid();

    var closeResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/settlement-periods/{periodId}/close",
      new { },
      cancellationToken);
    closeResponse.EnsureSuccessStatusCode();

    var recalculateResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/user-settlements/{settlementId}/recalculate",
      new { },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, recalculateResponse.StatusCode);
  }

  [Fact]
  public async Task CreateSettlementPeriod_WhenEmployeeLoggedTime_ShouldCalculateBalance()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    const int loggedMinutes = 600;
    var workDate = DateOnly.FromDateTime(DateTime.UtcNow);
    var year = workDate.Year;
    var month = workDate.Month;

    await SeedEmploymentWithContractAsync(admin, employee, year, month, cancellationToken);

    var projectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);
    await TimeTestHelper.CreateTimeEntryAsync(
      employee.Client,
      projectId,
      cancellationToken,
      workDate: workDate,
      durationMinutes: loggedMinutes);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = year,
      Month = month,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(createBody);
    var settlement = document.RootElement
      .GetProperty("value")
      .GetProperty("userSettlements")
      .EnumerateArray()
      .First(item => item.GetProperty("userId").GetGuid() == employee.UserId);

    var expectedMinutes = settlement.GetProperty("expectedMinutes").GetInt32();
    Assert.Equal(loggedMinutes, settlement.GetProperty("loggedMinutes").GetInt32());
    Assert.Equal(loggedMinutes - expectedMinutes, settlement.GetProperty("balanceMinutes").GetInt32());
    Assert.True(expectedMinutes > 0);
  }

  [Fact]
  public async Task GetMySettlements_WhenPeriodClosed_ShouldReturnPublishedSettlement()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    const int year = 2026;
    const int month = 3;

    await SeedEmploymentWithContractAsync(admin, employee, year, month, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = year,
      Month = month,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createDocument = JsonDocument.Parse(createBody);
    var periodId = createDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var closeResponse = await admin.Client.PostAsJsonAsync(
      $"/api/billing/settlement-periods/{periodId}/close",
      new { },
      cancellationToken);
    closeResponse.EnsureSuccessStatusCode();

    var mySettlementsResponse = await employee.Client.GetAsync(
      "/api/billing/my/settlements",
      cancellationToken);
    mySettlementsResponse.EnsureSuccessStatusCode();

    var body = await mySettlementsResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var settlement = document.RootElement
      .GetProperty("value")
      .EnumerateArray()
      .First(item => item.GetProperty("year").GetInt32() == year
                     && item.GetProperty("month").GetInt32() == month);

    Assert.True(settlement.GetProperty("expectedMinutes").GetInt32() > 0);
    Assert.Equal("March 2026", settlement.GetProperty("periodLabel").GetString());
  }

  private static async Task SeedEmploymentWithContractAsync(
    AuthenticatedTestUser admin,
    AuthenticatedTestUser employee,
    int year,
    int month,
    CancellationToken cancellationToken)
  {
    var positionResponse = await admin.Client.PostAsJsonAsync("/api/billing/positions", new
    {
      Name = $"Position-{Guid.NewGuid():N}"[..20],
      Department = "Ops",
      Description = (string?)null,
      IsActive = true,
    }, cancellationToken);
    positionResponse.EnsureSuccessStatusCode();

    var positionJson = await positionResponse.Content.ReadAsStringAsync(cancellationToken);
    using var positionDocument = JsonDocument.Parse(positionJson);
    var positionId = positionDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var monthStart = new DateOnly(year, month, 1);
    var contractResponse = await admin.Client.PostAsJsonAsync(
      $"/api/users/{employee.UserId}/employment/contracts",
      new
      {
        PositionId = positionId,
        ContractType = "Employment",
        StartDate = monthStart.ToString("yyyy-MM-dd"),
        EndDate = (string?)null,
        Fte = 1.00m,
        MonthlyHoursNormMinutes = 9600,
        HourlyRate = 100.00m,
        MonthlySalary = (decimal?)null,
        Notes = "Settlement test contract",
      },
      cancellationToken);
    contractResponse.EnsureSuccessStatusCode();
  }
}
