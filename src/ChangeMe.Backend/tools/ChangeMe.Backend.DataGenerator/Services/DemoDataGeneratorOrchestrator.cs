using ChangeMe.Backend.DataGenerator.Generators;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.DataGenerator.Services;

internal sealed class DemoDataGeneratorOrchestrator(
  ApplicationDbContext dbContext,
  UsersGenerator usersGenerator,
  ProjectsGenerator projectsGenerator,
  IssuesGenerator issuesGenerator,
  TimeEntriesGenerator timeEntriesGenerator,
  BillingGenerator billingGenerator,
  NotificationsGenerator notificationsGenerator,
  IOptions<DataGeneratorOptions> options,
  ILogger<DemoDataGeneratorOrchestrator> logger)
{
  public async Task<GenerationSummary> GenerateAsync(CancellationToken cancellationToken)
  {
    var config = options.Value;

    var users = await usersGenerator.GenerateAsync(cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);

    var billing = await billingGenerator.GenerateAsync(users, cancellationToken);

    await ProjectDefaultSeeder.EnsureDefaultProjectAsync(dbContext, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);

    var defaultProject = await dbContext.Projects
      .AsNoTracking()
      .Where(p => p.NormalizedName == Project.NormalizeName(ProjectConstraints.DefaultProjectName))
      .Select(p => new DemoProject(p.Id, p.Name))
      .FirstAsync(cancellationToken);

    var demoProjects = await projectsGenerator.GenerateAsync(users, cancellationToken);
    var allProjects = new List<DemoProject> { defaultProject };
    allProjects.AddRange(demoProjects);
    var projectIds = allProjects.Select(p => p.Id).ToList();

    var issues = issuesGenerator.Generate(users, projectIds);

    dbContext.Issues.AddRange(issues);
    await dbContext.SaveChangesAsync(cancellationToken);

    var timeEntries = timeEntriesGenerator.Generate(users, allProjects, issues);
    if (timeEntries.Count > 0)
    {
      var auditEntries = timeEntriesGenerator.BuildAuditEntries(timeEntries, allProjects, issues);
      dbContext.TimeEntries.AddRange(timeEntries);
      dbContext.TimeEntryAuditLog.AddRange(auditEntries);
      await dbContext.SaveChangesAsync(cancellationToken);
    }

    var notifications = notificationsGenerator.Generate(users, issues);
    if (notifications.Count > 0)
    {
      dbContext.Notifications.AddRange(notifications);
      await dbContext.SaveChangesAsync(cancellationToken);
    }

    var summary = new GenerationSummary(
      users.Count,
      demoProjects.Count,
      issues.Count,
      issues.Sum(i => i.Comments.Count),
      timeEntries.Count,
      notifications.Count,
      billing.ProfileCount,
      billing.ContractCount,
      billing.PatternCount,
      billing.RecurringEntryCount + billing.ManualEntryCount,
      billing.LeaveRequestCount,
      users.Select(u => u.Email).ToList(),
      config.DefaultPassword);

    logger.LogInformation(
      "Demo data generated: {UserCount} users, {ProjectCount} demo projects, {IssueCount} issues, {CommentCount} comments, {TimeEntryCount} time entries, {NotificationCount} notifications, {EmploymentProfileCount} employment profiles, {EmploymentContractCount} contracts, {AvailabilityPatternCount} weekly patterns, {AvailabilityEntryCount} availability entries, {LeaveRequestCount} leave requests",
      summary.UserCount,
      summary.ProjectCount,
      summary.IssueCount,
      summary.CommentCount,
      summary.TimeEntryCount,
      summary.NotificationCount,
      summary.EmploymentProfileCount,
      summary.EmploymentContractCount,
      summary.AvailabilityPatternCount,
      summary.AvailabilityEntryCount,
      summary.LeaveRequestCount);

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
  int TimeEntryCount,
  int NotificationCount,
  int EmploymentProfileCount,
  int EmploymentContractCount,
  int AvailabilityPatternCount,
  int AvailabilityEntryCount,
  int LeaveRequestCount,
  IReadOnlyList<string> DemoEmails,
  string DefaultPassword);
