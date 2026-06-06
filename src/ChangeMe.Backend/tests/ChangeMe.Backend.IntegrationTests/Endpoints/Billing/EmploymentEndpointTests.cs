using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Billing;

[Collection(IntegrationTestCollection.Name)]
public sealed class EmploymentEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PutEmploymentProfile_WhenAdministrator_ShouldUpsertProfile()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var employeeId = $"EMP-{Guid.NewGuid():N}"[..12];

    var response = await admin.Client.PutAsJsonAsync(
      $"/api/users/{user.UserId}/employment/profile",
      new
      {
        EmployeeId = employeeId,
        NationalId = "12345678901",
        TaxId = "1234567890",
        BankAccount = "PL61109010140000071219812874",
        Notes = "Demo employment profile",
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var getResponse = await admin.Client.GetAsync(
      $"/api/users/{user.UserId}/employment",
      cancellationToken);
    getResponse.EnsureSuccessStatusCode();

    var body = await getResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(employeeId, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostEmploymentContract_WhenAdministrator_ShouldCreateContract()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var positionName = $"Position-{Guid.NewGuid():N}";

    var positionResponse = await admin.Client.PostAsJsonAsync("/api/billing/positions", new
    {
      Name = positionName,
      Department = "HR",
      Description = (string?)null,
      IsActive = true,
    }, cancellationToken);
    positionResponse.EnsureSuccessStatusCode();

    var positionJson = await positionResponse.Content.ReadAsStringAsync(cancellationToken);
    using var positionDocument = JsonDocument.Parse(positionJson);
    var positionId = positionDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{user.UserId}/employment/contracts",
      new
      {
        PositionId = positionId,
        ContractType = "Employment",
        StartDate = "2026-01-01",
        EndDate = (string?)null,
        Fte = 1.00m,
        MonthlyHoursNormMinutes = 9600,
        HourlyRate = 120.00m,
        MonthlySalary = (decimal?)null,
        Notes = "Primary contract",
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var employmentResponse = await admin.Client.GetAsync(
      $"/api/users/{user.UserId}/employment",
      cancellationToken);
    employmentResponse.EnsureSuccessStatusCode();

    var employmentBody = await employmentResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(positionName, employmentBody, StringComparison.Ordinal);
    Assert.Contains("Active", employmentBody, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostEmploymentContract_WhenDatesOverlapExistingContract_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var positionId = await CreatePositionAsync(admin, cancellationToken);

    var firstResponse = await admin.Client.PostAsJsonAsync(
      $"/api/users/{user.UserId}/employment/contracts",
      new
      {
        PositionId = positionId,
        ContractType = "Employment",
        StartDate = "2026-01-01",
        EndDate = (string?)null,
        Fte = 1.00m,
        MonthlyHoursNormMinutes = 9600,
        HourlyRate = 120.00m,
        MonthlySalary = (decimal?)null,
        Notes = "Primary contract",
      },
      cancellationToken);
    firstResponse.EnsureSuccessStatusCode();

    var secondResponse = await admin.Client.PostAsJsonAsync(
      $"/api/users/{user.UserId}/employment/contracts",
      new
      {
        PositionId = positionId,
        ContractType = "Employment",
        StartDate = "2026-06-01",
        EndDate = (string?)null,
        Fte = 0.50m,
        MonthlyHoursNormMinutes = 4800,
        HourlyRate = 80.00m,
        MonthlySalary = (decimal?)null,
        Notes = "Overlapping contract",
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    var body = await secondResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(BillingConstraints.ContractOverlapMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostEmploymentContract_WhenFirstContractWithHalfFte_ShouldSeedScaledWeeklyPattern()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var positionId = await CreatePositionAsync(admin, cancellationToken);

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{user.UserId}/employment/contracts",
      new
      {
        PositionId = positionId,
        ContractType = "Employment",
        StartDate = "2026-01-01",
        EndDate = (string?)null,
        Fte = 0.50m,
        MonthlyHoursNormMinutes = 4800,
        HourlyRate = 80.00m,
        MonthlySalary = (decimal?)null,
        Notes = "Half-time contract",
      },
      cancellationToken);
    response.EnsureSuccessStatusCode();

    var patternResponse = await admin.Client.GetAsync(
      $"/api/billing/users/{user.UserId}/availability/pattern",
      cancellationToken);
    patternResponse.EnsureSuccessStatusCode();

    var body = await patternResponse.Content.ReadAsStringAsync(cancellationToken);
    using var document = JsonDocument.Parse(body);
    var monday = document.RootElement
      .GetProperty("value")
      .GetProperty("days")
      .EnumerateArray()
      .First(day => day.GetProperty("dayOfWeek").GetString() == DayOfWeek.Monday.ToString());

    Assert.True(monday.GetProperty("enabled").GetBoolean());
    Assert.Equal("09:00:00", monday.GetProperty("startTime").GetString());
    Assert.Equal("13:00:00", monday.GetProperty("endTime").GetString());
  }

  [Fact]
  public async Task GetUserEmployment_WhenViewerHasViewAnyOnly_ShouldMaskBankAccount()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var employee = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    const string bankAccount = "PL61109010140000071219812874";

    var profileResponse = await admin.Client.PutAsJsonAsync(
      $"/api/users/{employee.UserId}/employment/profile",
      new
      {
        EmployeeId = $"EMP-{Guid.NewGuid():N}"[..12],
        NationalId = "12345678901",
        TaxId = "1234567890",
        BankAccount = bankAccount,
        Notes = "Masking test profile",
      },
      cancellationToken);
    profileResponse.EnsureSuccessStatusCode();

    var viewer = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(
      factory,
      RoleConstraints.UserRoleName,
      cancellationToken);
    var viewerRoleId = await RolesTestHelper.CreateCustomRoleAsync(
      admin.Client,
      cancellationToken,
      permissionCodes: new[] { PermissionCodes.BillingViewAny, PermissionCodes.UsersView });
    await RolesTestHelper.AssignUserRolesAsync(
      admin.Client,
      viewer.UserId,
      viewer.Email,
      new[] { userRoleId, viewerRoleId },
      cancellationToken);
    await TestAuthHelper.RefreshLoginAsync(factory, viewer, cancellationToken);

    var maskedResponse = await viewer.Client.GetAsync(
      $"/api/users/{employee.UserId}/employment",
      cancellationToken);
    maskedResponse.EnsureSuccessStatusCode();

    var maskedBody = await maskedResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("***2874", maskedBody, StringComparison.Ordinal);
    Assert.DoesNotContain(bankAccount, maskedBody, StringComparison.Ordinal);

    var adminResponse = await admin.Client.GetAsync(
      $"/api/users/{employee.UserId}/employment",
      cancellationToken);
    adminResponse.EnsureSuccessStatusCode();

    var adminBody = await adminResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(bankAccount, adminBody, StringComparison.Ordinal);
  }

  private static async Task<Guid> CreatePositionAsync(
    AuthenticatedTestUser admin,
    CancellationToken cancellationToken)
  {
    var positionResponse = await admin.Client.PostAsJsonAsync("/api/billing/positions", new
    {
      Name = $"Position-{Guid.NewGuid():N}",
      Department = "HR",
      Description = (string?)null,
      IsActive = true,
    }, cancellationToken);
    positionResponse.EnsureSuccessStatusCode();

    var positionJson = await positionResponse.Content.ReadAsStringAsync(cancellationToken);
    using var positionDocument = JsonDocument.Parse(positionJson);
    return positionDocument.RootElement.GetProperty("value").GetProperty("id").GetGuid();
  }
}
