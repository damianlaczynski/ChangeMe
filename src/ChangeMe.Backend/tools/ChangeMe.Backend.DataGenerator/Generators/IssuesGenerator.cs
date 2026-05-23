using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using Microsoft.Extensions.Options;
using DomainIssue = global::ChangeMe.Backend.Domain.Aggregates.Issue.Issue;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;
using IssueConstraints = global::ChangeMe.Backend.Domain.Aggregates.Issue.IssueConstraints;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class IssuesGenerator(IOptions<DataGeneratorOptions> options)
{
  public IReadOnlyList<DomainIssue> Generate(IReadOnlyList<DomainUser> demoUsers)
  {
    var config = options.Value;
    var faker = new Faker { Random = new Randomizer(config.Seed + 1) };
    var issues = new List<DomainIssue>();
    var statuses = Enum.GetValues<IssueStatus>();
    var priorities = Enum.GetValues<IssuePriority>();

    for (var index = 0; index < config.Issues; index++)
    {
      var actor = PickRandom(demoUsers, faker);
      var assignee = faker.Random.Bool(0.7f) ? PickRandom(demoUsers, faker) : null;
      var title = Truncate(faker.Hacker.Phrase(), IssueConstraints.TITLE_MAX_LENGTH);
      if (title.Length < IssueConstraints.TITLE_MIN_LENGTH)
        title = $"Issue {index + 1}: {title}";

      var description = Truncate(faker.Lorem.Paragraphs(1, 2), IssueConstraints.DESCRIPTION_MAX_LENGTH);
      var priority = faker.PickRandom(priorities);
      var status = faker.PickRandom(statuses);

      var issueResult = DomainIssue.Create(
        title,
        description,
        priority,
        status,
        assignee?.Id);

      if (!issueResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create demo issue: {string.Join(", ", issueResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var issue = issueResult.Value;
      issue.RecordCreation(actor.Id);

      var criteriaCount = faker.Random.Int(0, 3);
      for (var c = 0; c < criteriaCount; c++)
      {
        var criterion = Truncate(faker.Lorem.Sentence(4), IssueAcceptanceCriterionConstraints.CONTENT_MAX_LENGTH);
        var criterionResult = issue.AddAcceptanceCriterion(criterion, actor.Id);
        if (!criterionResult.IsSuccess)
          throw new InvalidOperationException($"Failed to add acceptance criterion: {string.Join(", ", criterionResult.ValidationErrors.Select(e => e.ErrorMessage))}");
      }

      var commentCount = faker.Random.Int(config.CommentsPerIssueMin, config.CommentsPerIssueMax);
      for (var c = 0; c < commentCount; c++)
      {
        var commentAuthor = PickRandom(demoUsers, faker);
        var content = Truncate(faker.Lorem.Paragraph(), IssueCommentConstraints.CONTENT_MAX_LENGTH);
        var commentResult = issue.AddComment(content);
        if (!commentResult.IsSuccess)
          throw new InvalidOperationException($"Failed to add comment: {string.Join(", ", commentResult.ValidationErrors.Select(e => e.ErrorMessage))}");

        EntityAudit.Apply(commentResult.Value, commentAuthor.Id);
      }

      if (faker.Random.Bool(0.4f))
      {
        var watcher = PickRandom(demoUsers, faker);
        issue.StartWatching(watcher.Id);
      }

      EntityAudit.Apply(issue, actor.Id);
      EntityAudit.Apply(issue.HistoryEntries, actor.Id);
      EntityAudit.Apply(issue.AcceptanceCriteria, actor.Id);
      EntityAudit.Apply(issue.Comments, actor.Id);
      EntityAudit.Apply(issue.Watchers, actor.Id);

      issues.Add(issue);
    }

    return issues;
  }

  private static T PickRandom<T>(IReadOnlyList<T> items, Faker faker) =>
    items[faker.Random.Int(0, items.Count - 1)];

  private static string Truncate(string value, int maxLength)
  {
    var trimmed = value.Trim();
    return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
  }
}
