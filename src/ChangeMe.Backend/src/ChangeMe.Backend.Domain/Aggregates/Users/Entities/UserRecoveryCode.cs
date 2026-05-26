namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class UserRecoveryCode
{
  private UserRecoveryCode() { }

  public Guid Id { get; private set; } = Guid.CreateVersion7();
  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public string CodeHash { get; private set; } = string.Empty;
  public DateTime? UsedAtUtc { get; private set; }

  public bool IsUsed => UsedAtUtc is not null;

  public static Result<UserRecoveryCode> Create(Guid userId, string codeHash)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(codeHash))
      validationErrors.Add(new ValidationError(nameof(CodeHash), "cannot be null or empty"));
    else if (codeHash.Length > TwoFactorConstraints.RECOVERY_CODE_HASH_MAX_LENGTH)
      validationErrors.Add(new ValidationError(
        nameof(CodeHash),
        $"cannot be longer than {TwoFactorConstraints.RECOVERY_CODE_HASH_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new UserRecoveryCode
    {
      UserId = userId,
      CodeHash = codeHash.Trim()
    });
  }

  public void MarkUsed(DateTime utcNow) => UsedAtUtc = utcNow;
}
