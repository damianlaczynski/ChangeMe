using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class BillingReportEndpointsTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetSettlementReport_WhenAdministrator_ShouldReturnByPersonRows()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await SeedEmploymentWithContractAsync(admin, employee, 2026, 12, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = 2026,
      Month = 12,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createDocument = JsonDocument.Parse(createBody);
    var periodId = createDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var response = await admin.Client.GetAsync(
      $"/api/billing/reports/settlements?settlementPeriodId={periodId}&groupingMode=ByPerson",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var value = document.RootElement.GetProperty("value");
    Assert.True(value.GetProperty("rows").GetArrayLength() >= 1);
    Assert.Equal("ByPerson", value.GetProperty("groupingMode").GetString());
  }

  [Fact]
  public async Task GetLeaveReport_WhenAdministrator_ShouldReturnApprovedCalendarRows()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.GetAsync(
      "/api/billing/reports/leave?year=2026&groupingMode=LeaveCalendar&statuses=Approved",
      cancellationToken);

    response.EnsureSuccessStatusCode();

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var value = document.RootElement.GetProperty("value");
    Assert.Equal("LeaveCalendar", value.GetProperty("groupingMode").GetString());
    Assert.Equal(2026, value.GetProperty("year").GetInt32());
  }

  [Fact]
  public async Task GetSettlementOperationHistory_WhenAdministrator_ShouldReturnPagedEntries()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/settlement-periods", new
    {
      Year = 2026,
      Month = 7,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createDocument = JsonDocument.Parse(createBody);
    var periodId = createDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    await admin.Client.PostAsJsonAsync(
      $"/api/billing/settlement-periods/{periodId}/close",
      new { },
      cancellationToken);

    var response = await admin.Client.GetAsync(
      "/api/billing/settlement-operation-history?pageNumber=1&pageSize=10",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var value = document.RootElement.GetProperty("value");
    Assert.True(value.GetProperty("totalCount").GetInt32() >= 1);
    Assert.NotEmpty(value.GetProperty("items").EnumerateArray());
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
        Notes = "Report test contract",
      },
      cancellationToken);
    contractResponse.EnsureSuccessStatusCode();
  }
}
