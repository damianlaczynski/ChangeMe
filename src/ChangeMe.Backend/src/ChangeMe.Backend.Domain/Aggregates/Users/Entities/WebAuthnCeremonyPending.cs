namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public enum WebAuthnCeremonyType
{
  Registration = 0,
  Authentication = 1,
  StepUp = 2
}

public class WebAuthnCeremonyPending
{
  private WebAuthnCeremonyPending() { }

  public Guid Id { get; private set; } = Guid.CreateVersion7();
  public Guid? UserId { get; private set; }
  public string? NormalizedEmail { get; private set; }
  public WebAuthnCeremonyType CeremonyType { get; private set; }
  public string OptionsJson { get; private set; } = string.Empty;
  public int FailedAttemptCount { get; private set; }
  public DateTime ExpiresAtUtc { get; private set; }

  public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

  public static Result<WebAuthnCeremonyPending> Create(
    WebAuthnCeremonyType ceremonyType,
    string optionsJson,
    DateTime expiresAtUtc,
    Guid? userId = null,
    string? normalizedEmail = null)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(optionsJson))
      validationErrors.Add(new ValidationError(nameof(OptionsJson), "cannot be null or empty"));
    else if (optionsJson.Length > PasskeyConstraints.CEREMONY_OPTIONS_JSON_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(OptionsJson), "is too long"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new WebAuthnCeremonyPending
    {
      CeremonyType = ceremonyType,
      OptionsJson = optionsJson,
      ExpiresAtUtc = expiresAtUtc,
      UserId = userId,
      NormalizedEmail = normalizedEmail
    });
  }

  public void RecordFailedAttempt() => FailedAttemptCount++;
}
