using ChangeMe.Backend.Domain.Aggregates.Users.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Users.Entities;

public class ExternalAuthPending
{
  private ExternalAuthPending() { }

  public Guid Id { get; private set; } = Guid.CreateVersion7();
  public string ProviderKey { get; private set; } = string.Empty;
  public string State { get; private set; } = string.Empty;
  public string Nonce { get; private set; } = string.Empty;
  public string CodeChallenge { get; private set; } = string.Empty;
  public string CodeVerifier { get; private set; } = string.Empty;
  public ExternalAuthMode Mode { get; private set; }
  public Guid? UserId { get; private set; }
  public string? ProviderSubject { get; private set; }
  public string? ProviderEmail { get; private set; }
  public bool ProviderEmailVerified { get; private set; }
  public string? ProviderFirstName { get; private set; }
  public string? ProviderLastName { get; private set; }
  public bool IdentityProviderMfaAsserted { get; private set; }
  public DateTime ExpiresAtUtc { get; private set; }
  public string? InvitedProfileEmail { get; private set; }

  public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

  public static Result<ExternalAuthPending> CreateSignIn(
    string providerKey,
    string state,
    string nonce,
    string codeChallenge,
    string codeVerifier,
    DateTime expiresAtUtc,
    string? invitedProfileEmail = null)
  {
    var validationErrors = ValidateCore(providerKey, state, nonce, codeChallenge, codeVerifier);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new ExternalAuthPending
    {
      ProviderKey = providerKey.Trim(),
      State = state,
      Nonce = nonce,
      CodeChallenge = codeChallenge,
      CodeVerifier = codeVerifier,
      Mode = ExternalAuthMode.SignIn,
      ExpiresAtUtc = expiresAtUtc,
      InvitedProfileEmail = string.IsNullOrWhiteSpace(invitedProfileEmail)
        ? null
        : invitedProfileEmail.Trim()
    });
  }

  public static Result<ExternalAuthPending> CreateLink(
    string providerKey,
    string state,
    string nonce,
    string codeChallenge,
    string codeVerifier,
    Guid userId,
    DateTime expiresAtUtc)
  {
    var validationErrors = ValidateCore(providerKey, state, nonce, codeChallenge, codeVerifier);
    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(userId), "cannot be empty"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new ExternalAuthPending
    {
      ProviderKey = providerKey.Trim(),
      State = state,
      Nonce = nonce,
      CodeChallenge = codeChallenge,
      CodeVerifier = codeVerifier,
      Mode = ExternalAuthMode.Link,
      UserId = userId,
      ExpiresAtUtc = expiresAtUtc
    });
  }

  public void SetProviderAssertion(
    string providerSubject,
    string? providerEmail,
    bool providerEmailVerified,
    string? providerFirstName,
    string? providerLastName,
    bool identityProviderMfaAsserted)
  {
    ProviderSubject = providerSubject.Trim();
    ProviderEmail = string.IsNullOrWhiteSpace(providerEmail) ? null : providerEmail.Trim();
    ProviderEmailVerified = providerEmailVerified;
    ProviderFirstName = string.IsNullOrWhiteSpace(providerFirstName) ? null : providerFirstName.Trim();
    ProviderLastName = string.IsNullOrWhiteSpace(providerLastName) ? null : providerLastName.Trim();
    IdentityProviderMfaAsserted = identityProviderMfaAsserted;
  }

  public static Result<ExternalAuthPending> CreateStepUp(
    string providerKey,
    string state,
    string nonce,
    string codeChallenge,
    string codeVerifier,
    Guid userId,
    DateTime expiresAtUtc)
  {
    var validationErrors = ValidateCore(providerKey, state, nonce, codeChallenge, codeVerifier);
    if (userId == Guid.Empty)
      validationErrors.Add(new ValidationError(nameof(userId), "cannot be empty"));

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success(new ExternalAuthPending
    {
      ProviderKey = providerKey.Trim(),
      State = state,
      Nonce = nonce,
      CodeChallenge = codeChallenge,
      CodeVerifier = codeVerifier,
      Mode = ExternalAuthMode.StepUp,
      UserId = userId,
      ExpiresAtUtc = expiresAtUtc
    });
  }

  public void MarkLinkAccountRequired()
  {
    Mode = ExternalAuthMode.LinkAccount;
  }

  private static List<ValidationError> ValidateCore(
    string providerKey,
    string state,
    string nonce,
    string codeChallenge,
    string codeVerifier)
  {
    var validationErrors = new List<ValidationError>();

    if (string.IsNullOrWhiteSpace(providerKey))
      validationErrors.Add(new ValidationError(nameof(providerKey), "cannot be null or empty"));

    if (string.IsNullOrWhiteSpace(state))
      validationErrors.Add(new ValidationError(nameof(state), "cannot be null or empty"));

    if (string.IsNullOrWhiteSpace(nonce))
      validationErrors.Add(new ValidationError(nameof(nonce), "cannot be null or empty"));

    if (string.IsNullOrWhiteSpace(codeChallenge))
      validationErrors.Add(new ValidationError(nameof(codeChallenge), "cannot be null or empty"));

    if (string.IsNullOrWhiteSpace(codeVerifier))
      validationErrors.Add(new ValidationError(nameof(codeVerifier), "cannot be null or empty"));

    return validationErrors;
  }
}
