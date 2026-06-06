using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Aggregates.Time.Entities;
using Microsoft.Extensions.Options;
using DomainIssue = global::ChangeMe.Backend.Domain.Aggregates.Issue.Issue;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class TimeEntriesGenerator(IOptions<DataGeneratorOptions> options)
{
  public IReadOnlyList<TimeEntry> Generate(
    IReadOnlyList<DomainUser> demoUsers,
    IReadOnlyList<DemoProject> allProjects,
    IReadOnlyList<DomainIssue> issues)
  {
    if (demoUsers.Count == 0 || allProjects.Count == 0)
      return [];

    var config = options.Value;
    if (config.TimeEntries <= 0)
      return [];

    var faker = new Faker { Random = new Randomizer(config.Seed + 3) };
    var issuesByProject = issues
      .GroupBy(i => i.ProjectId)
      .ToDictionary(g => g.Key, g => g.ToList());
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var earliestWorkDate = today.AddDays(-Math.Min(TimeConstraints.DefaultBackdatingLimitDays - 1, 28));
    var entries = new List<TimeEntry>();

    for (var index = 0; index < config.TimeEntries; index++)
    {
      var author = PickRandom(demoUsers, faker);
      var project = PickRandom(allProjects, faker);
      var issue = PickOptionalIssue(project.Id, issuesByProject, faker);
      var workDate = DateOnly.FromDayNumber(
        faker.Random.Int(earliestWorkDate.DayNumber, today.DayNumber));
      var durationMinutes = faker.Random.Int(1, 32) * 15;
      if (durationMinutes > TimeConstraints.MaxDurationMinutes)
        durationMinutes = TimeConstraints.MaxDurationMinutes;

      var description = Truncate(faker.Hacker.Phrase(), TimeConstraints.DescriptionMaxLength);

      var entryResult = TimeEntry.Create(
        author.Id,
        project.Id,
        issue?.Id,
        workDate,
        durationMinutes,
        description);
      if (!entryResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create demo time entry: {string.Join(", ", entryResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var entry = entryResult.Value;
      EntityAudit.Apply(entry, author.Id);
      entries.Add(entry);
    }

    return entries;
  }

  public IReadOnlyList<TimeEntryAuditLogEntry> BuildAuditEntries(
    IReadOnlyList<TimeEntry> entries,
    IReadOnlyList<DemoProject> allProjects,
    IReadOnlyList<DomainIssue> issues)
  {
    var projectNames = allProjects.ToDictionary(p => p.Id, p => p.Name);
    var issueTitles = issues.ToDictionary(i => i.Id, i => i.Title);

    return entries
      .Select(entry =>
      {
        var projectName = projectNames.GetValueOrDefault(entry.ProjectId, "Unknown project");
        string? issueTitle = null;
        if (entry.IssueId is Guid issueId)
          issueTitles.TryGetValue(issueId, out issueTitle);

        var auditEntry = TimeEntryAuditLogEntry.ForCreate(
          entry.Id,
          entry.AuthorUserId,
          entry.AuthorUserId,
          entry.ProjectId,
          projectName,
          entry.IssueId,
          issueTitle,
          entry.WorkDate,
          entry.DurationMinutes,
          entry.Description);

        EntityAudit.Apply(auditEntry, entry.AuthorUserId);
        return auditEntry;
      })
      .ToList();
  }

  private static DomainIssue? PickOptionalIssue(
    Guid projectId,
    IReadOnlyDictionary<Guid, List<DomainIssue>> issuesByProject,
    Faker faker)
  {
    if (!issuesByProject.TryGetValue(projectId, out var projectIssues) || projectIssues.Count == 0)
      return null;

    return faker.Random.Bool(0.65f) ? PickRandom(projectIssues, faker) : null;
  }

  private static T PickRandom<T>(IReadOnlyList<T> items, Faker faker) =>
    items[faker.Random.Int(0, items.Count - 1)];

  private static string Truncate(string value, int maxLength)
  {
    var trimmed = value.Trim();
    return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
  }
}
