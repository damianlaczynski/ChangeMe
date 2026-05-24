namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class SignInChallenge
{
  private SignInChallenge() { }

  public Guid Id { get; private set; } = Guid.CreateVersion7();
  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public int FailedAttemptCount { get; private set; }
  public DateTime ExpiresAtUtc { get; private set; }

  public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

  public static Result<SignInChallenge> Create(
    Guid userId,
    DateTime expiresAtUtc)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new SignInChallenge
    {
      UserId = userId,
      ExpiresAtUtc = expiresAtUtc
    });
  }

  public void RecordFailedAttempt() => FailedAttemptCount++;
}
