using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class PositionsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPosition_WhenAdministrator_ShouldCreatePosition()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var name = $"Position-{Guid.NewGuid():N}";

    var response = await admin.Client.PostAsJsonAsync("/api/billing/positions", new
    {
      Name = name,
      Department = "Engineering",
      Description = "Demo position",
      IsActive = true,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(name, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetPositions_WhenAdministrator_ShouldReturnCreatedPosition()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var name = $"Listed-{Guid.NewGuid():N}";

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/positions", new
    {
      Name = name,
      Department = (string?)null,
      Description = (string?)null,
      IsActive = true,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var listResponse = await admin.Client.GetAsync(
      $"/api/billing/positions?searchText={Uri.EscapeDataString(name)}&pageNumber=1&pageSize=10",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

    var body = await listResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(name, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task DeletePosition_WhenReferencedByContract_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var name = $"Referenced-{Guid.NewGuid():N}";

    var createResponse = await admin.Client.PostAsJsonAsync("/api/billing/positions", new
    {
      Name = name,
      Department = "Ops",
      Description = (string?)null,
      IsActive = true,
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    using var createDocument = JsonDocument.Parse(createBody);
    var positionId = createDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var contractResponse = await admin.Client.PostAsJsonAsync(
      $"/api/users/{employee.UserId}/employment/contracts",
      new
      {
        PositionId = positionId,
        ContractType = "Employment",
        StartDate = "2026-01-01",
        EndDate = (string?)null,
        Fte = 1.00m,
        MonthlyHoursNormMinutes = 9600,
        HourlyRate = 100.00m,
        MonthlySalary = (decimal?)null,
        Notes = "Position reference test",
      },
      cancellationToken);
    contractResponse.EnsureSuccessStatusCode();

    var deleteResponse = await admin.Client.DeleteAsync(
      $"/api/billing/positions/{positionId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
  }

  [Fact]
  public async Task GetPositions_WhenRegularUser_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/billing/positions", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }
}
