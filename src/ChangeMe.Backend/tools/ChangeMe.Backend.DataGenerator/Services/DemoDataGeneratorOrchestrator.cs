using ChangeMe.Backend.DataGenerator.Generators;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.DataGenerator.Services;

internal sealed class DemoDataGeneratorOrchestrator(
  ApplicationDbContext dbContext,
  UsersGenerator usersGenerator,
  ProjectsGenerator projectsGenerator,
  IssuesGenerator issuesGenerator,
  NotificationsGenerator notificationsGenerator,
  IOptions<DataGeneratorOptions> options,
  ILogger<DemoDataGeneratorOrchestrator> logger)
{
  public async Task<GenerationSummary> GenerateAsync(CancellationToken cancellationToken)
  {
    var config = options.Value;

    var users = await usersGenerator.GenerateAsync(cancellationToken);
    var projects = projectsGenerator.Generate(users);
    var issues = issuesGenerator.Generate(users, projects);

    dbContext.Projects.AddRange(projects);
    dbContext.ProjectMembers.AddRange(projects.SelectMany(p => p.Members));
    dbContext.Issues.AddRange(issues);
    await dbContext.SaveChangesAsync(cancellationToken);

    var notifications = notificationsGenerator.Generate(users, issues);
    if (notifications.Count > 0)
    {
      dbContext.Notifications.AddRange(notifications);
      await dbContext.SaveChangesAsync(cancellationToken);
    }

    var summary = new GenerationSummary(
      users.Count,
      projects.Count,
      issues.Count,
      issues.Sum(i => i.Comments.Count),
      notifications.Count,
      users.Select(u => u.Email).ToList(),
      config.DefaultPassword);

    logger.LogInformation(
      "Demo data generated: {UserCount} users, {ProjectCount} projects, {IssueCount} issues, {CommentCount} comments, {NotificationCount} notifications",
      summary.UserCount,
      summary.ProjectCount,
      summary.IssueCount,
      summary.CommentCount,
      summary.NotificationCount);

    foreach (var email in summary.DemoEmails)
      logger.LogInformation("Demo account: {Email} / {Password}", email, summary.DefaultPassword);

    return summary;
  }
}

internal sealed record GenerationSummary(
  int UserCount,
  int ProjectCount,
  int IssueCount,
  int CommentCount,
  int NotificationCount,
  IReadOnlyList<string> DemoEmails,
  string DefaultPassword);
