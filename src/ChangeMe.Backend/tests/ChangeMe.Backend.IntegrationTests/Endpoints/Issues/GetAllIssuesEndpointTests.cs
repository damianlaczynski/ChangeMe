using System.Net;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetAllIssuesEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAllIssues_WhenIssuesExist_ShouldReturnOkWithSeededItems()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    await IssueTestHelper.SeedIssueAsync(factory, "Issue one", "First", IssuePriority.LOW, null, cancellationToken);
    await IssueTestHelper.SeedIssueAsync(factory, "Issue two", "Second", IssuePriority.MEDIUM, null, cancellationToken);

    using var client = await TestAuthHelper.CreateAuthenticatedClientAsync(factory, cancellationToken);

    var response = await client.GetAsync("/api/issues?pageNumber=1&pageSize=10", cancellationToken);
    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Contains("Issue one", responseBody, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Issue two", responseBody, StringComparison.OrdinalIgnoreCase);
  }
}
