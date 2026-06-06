using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class TimeTestHelper
{
  public static async Task<Guid> CreateTimeEntryAsync(
    HttpClient client,
    Guid projectId,
    CancellationToken cancellationToken,
    Guid? issueId = null,
    DateOnly? workDate = null,
    int durationMinutes = 60,
    string? description = "Integration test time entry")
  {
    var response = await client.PostAsJsonAsync("/api/time/entries", new
    {
      ProjectId = projectId,
      IssueId = issueId,
      WorkDate = workDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
      DurationMinutes = durationMinutes,
      Description = description,
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    return RolesTestHelper.ReadGuidFromResultBody(body, "id");
  }
}
