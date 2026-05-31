using System.Reflection;
using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Domain.Common.Attachments;
using ChangeMe.Backend.Domain.Aggregates.Notifications;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence;

public class ApplicationDbContext(
  DbContextOptions<ApplicationDbContext> options,
  IDomainEventDispatcher? dispatcher = null,
  IUserAccessor? userAccessor = null) : DbContext(options)
{
  public DbSet<Issue> Issues => Set<Issue>();
  public DbSet<IssueAcceptanceCriterion> IssueAcceptanceCriteria => Set<IssueAcceptanceCriterion>();
  public DbSet<IssueComment> IssueComments => Set<IssueComment>();
  public DbSet<Attachment> Attachments => Set<Attachment>();
  public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();
  public DbSet<IssueHistoryEntry> IssueHistoryEntries => Set<IssueHistoryEntry>();
  public DbSet<IssueWatcher> IssueWatchers => Set<IssueWatcher>();
  public DbSet<Notification> Notifications => Set<Notification>();
  public DbSet<User> Users => Set<User>();
  public DbSet<Role> Roles => Set<Role>();
  public DbSet<UserSession> UserSessions => Set<UserSession>();
  public DbSet<UserAuthToken> UserAuthTokens => Set<UserAuthToken>();
  public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
  public DbSet<UserRecoveryCode> UserRecoveryCodes => Set<UserRecoveryCode>();
  public DbSet<SignInChallenge> SignInChallenges => Set<SignInChallenge>();
  public DbSet<TwoFactorEnrollmentPending> TwoFactorEnrollmentPending => Set<TwoFactorEnrollmentPending>();
  public DbSet<ExternalAuthPending> ExternalAuthPending => Set<ExternalAuthPending>();
  public DbSet<PasskeyCredential> PasskeyCredentials => Set<PasskeyCredential>();
  public DbSet<WebAuthnCeremonyPending> WebAuthnCeremonyPending => Set<WebAuthnCeremonyPending>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.HasDefaultSchema(DatabaseSchema.Default);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
  {
    UpdateTimeStamps();
    UpdateUserInfo();

    int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    DispatchDomainEvents();

    return result;
  }

  public override int SaveChanges()
  {
    UpdateTimeStamps();
    UpdateUserInfo();

    var result = base.SaveChanges();

    DispatchDomainEvents();

    return result;
  }

  private void UpdateTimeStamps()
  {
    var now = DateTime.UtcNow;

    var entries = ChangeTracker.Entries<Entity>()
      .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
        entry.Property(e => e.CreatedAt).CurrentValue = now;

      entry.Property(e => e.UpdatedAt).CurrentValue = now;
    }
  }

  private void UpdateUserInfo()
  {
    if (userAccessor?.UserId is not Guid currentUserId) return;

    var entries = ChangeTracker.Entries<Entity>()
      .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
        entry.Property(e => e.CreatedBy).CurrentValue = currentUserId;

      entry.Property(e => e.UpdatedBy).CurrentValue = currentUserId;
    }
  }

  private void DispatchDomainEvents()
  {
    if (dispatcher == null) return;

    var entitiesWithEvents = ChangeTracker.Entries<HasDomainEventsBase>()
      .Select(e => e.Entity)
      .Where(e => e.DomainEvents.Any())
      .ToArray();

    dispatcher.DispatchAndClearEvents(entitiesWithEvents);
  }
}
