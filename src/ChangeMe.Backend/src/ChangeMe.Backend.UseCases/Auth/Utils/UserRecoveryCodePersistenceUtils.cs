using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

internal static class UserRecoveryCodePersistenceUtils
{
  public static async Task DeleteAllForUserAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    await context.UserRecoveryCodes
      .Where(x => x.UserId == userId)
      .ExecuteDeleteAsync(cancellationToken);

    DetachTrackedRecoveryCodes(context, userId);
  }

  public static async Task ReplaceAllForUserAsync(
    ApplicationDbContext context,
    Guid userId,
    IReadOnlyList<UserRecoveryCode> codes,
    CancellationToken cancellationToken)
  {
    await DeleteAllForUserAsync(context, userId, cancellationToken);

    foreach (var code in codes)
      await context.UserRecoveryCodes.AddAsync(code, cancellationToken);
  }

  private static void DetachTrackedRecoveryCodes(ApplicationDbContext context, Guid userId)
  {
    foreach (var entry in context.ChangeTracker
      .Entries<UserRecoveryCode>()
      .Where(x => x.Entity.UserId == userId)
      .ToList())
    {
      entry.State = EntityState.Detached;
    }
  }
}
