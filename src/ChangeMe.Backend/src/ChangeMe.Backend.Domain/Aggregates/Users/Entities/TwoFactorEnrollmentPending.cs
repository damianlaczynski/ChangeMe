namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class TwoFactorEnrollmentPending
{
  private TwoFactorEnrollmentPending() { }

  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public string SecretCiphertext { get; private set; } = string.Empty;
  public DateTime ExpiresAtUtc { get; private set; }

  public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

  public static Result<TwoFactorEnrollmentPending> Create(
    Guid userId,
    string secretCiphertext,
    DateTime expiresAtUtc)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    if (string.IsNullOrWhiteSpace(secretCiphertext))
      validationErrors.Add(new ValidationError(nameof(SecretCiphertext), "cannot be null or empty"));
    else if (secretCiphertext.Length > TwoFactorConstraints.ENCRYPTED_SECRET_MAX_LENGTH)
      validationErrors.Add(new ValidationError(
        nameof(SecretCiphertext),
        $"cannot be longer than {TwoFactorConstraints.ENCRYPTED_SECRET_MAX_LENGTH} characters"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new TwoFactorEnrollmentPending
    {
      UserId = userId,
      SecretCiphertext = secretCiphertext.Trim(),
      ExpiresAtUtc = expiresAtUtc
    });
  }
}
