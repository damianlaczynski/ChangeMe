using ChangeMe.Backend.Domain.Aggregates.Sessions;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class SignInChallenge
{
  private SignInChallenge() { }

  public Guid Id { get; private set; } = Guid.CreateVersion7();
  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public int FailedAttemptCount { get; private set; }
  public DateTime ExpiresAtUtc { get; private set; }
  public string? PendingSignInMethod { get; private set; }

  public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

  public static Result<SignInChallenge> Create(
    Guid userId,
    DateTime expiresAtUtc,
    string pendingSignInMethod)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(pendingSignInMethod))
      validationErrors.Add(new ValidationError(nameof(PendingSignInMethod), "cannot be null or empty"));
    else if (pendingSignInMethod.Length > SessionConstraints.SIGN_IN_METHOD_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(PendingSignInMethod), "is too long"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new SignInChallenge
    {
      UserId = userId,
      ExpiresAtUtc = expiresAtUtc,
      PendingSignInMethod = pendingSignInMethod.Trim()
    });
  }

  public void RecordFailedAttempt() => FailedAttemptCount++;
}
