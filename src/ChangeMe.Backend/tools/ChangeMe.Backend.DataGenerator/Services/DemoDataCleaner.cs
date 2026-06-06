using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.DataGenerator.Services;

internal sealed class DemoDataCleaner(ApplicationDbContext dbContext, IOptions<DataGeneratorOptions> options)
{
  public async Task CleanAsync(CancellationToken cancellationToken)
  {
    var demoUserIds = await GetDemoUserIdsAsync(cancellationToken);
    if (demoUserIds.Count == 0)
      return;

    var demoProjectIds = await dbContext.Projects
      .Where(p => !p.IsSystem && demoUserIds.Contains(p.CreatedBy))
      .Select(p => p.Id)
      .ToListAsync(cancellationToken);

    var demoIssueIds = await dbContext.Issues
      .Where(i =>
        demoUserIds.Contains(i.CreatedBy)
        || (i.AssignedToUserId != null && demoUserIds.Contains(i.AssignedToUserId.Value))
        || demoProjectIds.Contains(i.ProjectId))
      .Select(i => i.Id)
      .ToListAsync(cancellationToken);

    await dbContext.Notifications
      .Where(n => demoUserIds.Contains(n.RecipientUserId)
                  || (n.IssueId.HasValue && demoIssueIds.Contains(n.IssueId.Value)))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.AvailabilityEntries
      .Where(e => demoUserIds.Contains(e.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.WeeklyRecurringPatterns
      .Where(p => demoUserIds.Contains(p.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.LeaveRequests
      .Where(r => demoUserIds.Contains(r.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.EmploymentContracts
      .Where(c => demoUserIds.Contains(c.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.EmploymentProfiles
      .Where(p => demoUserIds.Contains(p.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.TimeEntryAuditLog
      .Where(a =>
        demoUserIds.Contains(a.ActingUserId)
        || demoUserIds.Contains(a.EntryAuthorUserId)
        || demoProjectIds.Contains(a.ProjectId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.TimeEntries
      .Where(e =>
        demoUserIds.Contains(e.AuthorUserId)
        || demoProjectIds.Contains(e.ProjectId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.UserRunningTimers
      .Where(t => demoUserIds.Contains(t.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    if (demoIssueIds.Count > 0)
    {
      await dbContext.Issues
        .Where(i => demoIssueIds.Contains(i.Id))
        .ExecuteDeleteAsync(cancellationToken);
    }

    if (demoProjectIds.Count > 0)
    {
      await dbContext.ProjectMembers
        .Where(m => demoProjectIds.Contains(m.ProjectId))
        .ExecuteDeleteAsync(cancellationToken);

      await dbContext.ProjectMembershipHistory
        .Where(h => demoProjectIds.Contains(h.ProjectId))
        .ExecuteDeleteAsync(cancellationToken);

      await dbContext.ProjectOperationHistory
        .Where(h => demoProjectIds.Contains(h.ProjectId))
        .ExecuteDeleteAsync(cancellationToken);

      await dbContext.Projects
        .Where(p => demoProjectIds.Contains(p.Id))
        .ExecuteDeleteAsync(cancellationToken);
    }

    await dbContext.UserSessions
      .Where(s => demoUserIds.Contains(s.UserId))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.Users
      .Where(u => demoUserIds.Contains(u.Id))
      .ExecuteDeleteAsync(cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task<List<Guid>> GetDemoUserIdsAsync(CancellationToken cancellationToken)
  {
    var emailSuffix = $"@{options.Value.EmailDomain.Trim().ToUpperInvariant()}";

    return await dbContext.Users
      .Where(u => u.NormalizedEmail.EndsWith(emailSuffix))
      .Select(u => u.Id)
      .ToListAsync(cancellationToken);
  }
}
