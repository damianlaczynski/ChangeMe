namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class ExternalLogin
{
  private ExternalLogin() { }

  public Guid UserId { get; private set; }
  public User User { get; private set; } = null!;
  public string ProviderKey { get; private set; } = string.Empty;
  public string ProviderSubject { get; private set; } = string.Empty;
  public DateTime LinkedAtUtc { get; private set; }
  public DateTime? LastStepUpAtUtc { get; private set; }

  public static Result<ExternalLogin> Create(
    Guid userId,
    string providerKey,
    string providerSubject)
  {
    var validationErrors = new List<ValidationError>();

    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(UserId), "cannot be empty"));

    ValidateProviderKey(providerKey, validationErrors);
    ValidateProviderSubject(providerSubject, validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new ExternalLogin
    {
      UserId = userId,
      ProviderKey = providerKey.Trim(),
      ProviderSubject = providerSubject.Trim(),
      LinkedAtUtc = DateTime.UtcNow
    });
  }

  public void RecordStepUp(DateTime utcNow) => LastStepUpAtUtc = utcNow;

  private static void ValidateProviderKey(string providerKey, ICollection<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(providerKey))
    {
      validationErrors.Add(new ValidationError(nameof(ProviderKey), "cannot be null or empty"));
      return;
    }

    if (providerKey.Trim().Length > TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH)
      validationErrors.Add(new ValidationError(
        nameof(ProviderKey),
        $"cannot be longer than {TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH} characters"));
  }

  private static void ValidateProviderSubject(string providerSubject, ICollection<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(providerSubject))
    {
      validationErrors.Add(new ValidationError(nameof(ProviderSubject), "cannot be null or empty"));
      return;
    }

    if (providerSubject.Trim().Length > TwoFactorConstraints.PROVIDER_SUBJECT_MAX_LENGTH)
      validationErrors.Add(new ValidationError(
        nameof(ProviderSubject),
        $"cannot be longer than {TwoFactorConstraints.PROVIDER_SUBJECT_MAX_LENGTH} characters"));
  }
}
