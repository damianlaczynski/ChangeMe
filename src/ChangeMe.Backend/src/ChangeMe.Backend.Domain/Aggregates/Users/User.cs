using System.Net.Mail;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Domain.Aggregates.Users;

public class User : Entity, IAggregateRoot
{
  private readonly List<UserRole> roles = [];
  private readonly List<ExternalLogin> externalLogins = [];
  private readonly List<UserRecoveryCode> recoveryCodes = [];

  private User() { }

  public IReadOnlyCollection<UserRole> Roles => roles;
  public IReadOnlyCollection<ExternalLogin> ExternalLogins => externalLogins;
  public IReadOnlyCollection<UserRecoveryCode> RecoveryCodes => recoveryCodes;

  public string FirstName { get; private set; } = string.Empty;
  public string LastName { get; private set; } = string.Empty;
  public string Email { get; private set; } = string.Empty;
  public string NormalizedEmail { get; private set; } = string.Empty;
  public string PasswordHash { get; private set; } = string.Empty;
  public bool Deactivated { get; private set; }
  public DateTime? DeactivatedAt { get; private set; }
  public bool HasPasswordSet { get; private set; }
  public bool EmailVerified { get; private set; }
  public DateTime? EmailVerifiedAt { get; private set; }
  public DateTime? PasswordLastChangedAt { get; private set; }
  public DateTime? InvitationSentAt { get; private set; }
  public bool TwoFactorEnabled { get; private set; }
  public DateTime? TwoFactorEnabledAt { get; private set; }
  public string TwoFactorSecretCiphertext { get; private set; } = string.Empty;

  public bool IsActive => !Deactivated;
  public bool HasCompleteProfile =>
    !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);

  public string DisplayLabel => UserDisplayFormat.DisplayLabel(FirstName, LastName, Email);

  public static Result<User> CreateWithPassword(
    string firstName,
    string lastName,
    string email,
    string passwordHash,
    bool emailVerified = true)
  {
    var validationErrors = ValidateWithPassword(firstName, lastName, email, passwordHash);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var utcNow = DateTime.UtcNow;
    var user = new User
    {
      FirstName = firstName.Trim(),
      LastName = lastName.Trim(),
      Email = email.Trim(),
      NormalizedEmail = NormalizeEmail(email),
      PasswordHash = passwordHash.Trim(),
      HasPasswordSet = true,
      PasswordLastChangedAt = utcNow,
      EmailVerified = emailVerified,
      EmailVerifiedAt = emailVerified ? utcNow : null,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

    return Result.Success(user);
  }

  public static Result<User> CreateInvited(
    string email,
    string? firstName = null,
    string? lastName = null,
    bool emailVerified = true)
  {
    var validationErrors = new List<ValidationError>();
    ValidateEmail(email, validationErrors);
    ValidateOptionalName(firstName, nameof(FirstName), validationErrors);
    ValidateOptionalName(lastName, nameof(LastName), validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var utcNow = DateTime.UtcNow;
    var user = new User
    {
      FirstName = firstName?.Trim() ?? string.Empty,
      LastName = lastName?.Trim() ?? string.Empty,
      Email = email.Trim(),
      NormalizedEmail = NormalizeEmail(email),
      HasPasswordSet = false,
      EmailVerified = emailVerified,
      EmailVerifiedAt = emailVerified ? utcNow : null,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

    return Result.Success(user);
  }

  public Result UpdateProfile(string firstName, string lastName)
  {
    var validationErrors = new List<ValidationError>();
    ValidateName(firstName, nameof(FirstName), validationErrors);
    ValidateName(lastName, nameof(LastName), validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    FirstName = firstName.Trim();
    LastName = lastName.Trim();
    return Result.Success();
  }

  public Result UpdateAdminProfile(string firstName, string lastName, string email)
  {
    var validationErrors = new List<ValidationError>();

    if (HasPasswordSet)
    {
      ValidateName(firstName, nameof(FirstName), validationErrors);
      ValidateName(lastName, nameof(LastName), validationErrors);
    }
    else
    {
      ValidateOptionalName(firstName, nameof(FirstName), validationErrors);
      ValidateOptionalName(lastName, nameof(LastName), validationErrors);
    }

    ValidateEmail(email, validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    FirstName = firstName?.Trim() ?? string.Empty;
    LastName = lastName?.Trim() ?? string.Empty;
    Email = email.Trim();
    NormalizedEmail = NormalizeEmail(email);
    return Result.Success();
  }

  public Result SetPasswordHash(string newPasswordHash)
  {
    if (string.IsNullOrWhiteSpace(newPasswordHash))
      return Result.Invalid(new ValidationError(nameof(PasswordHash), "cannot be null or empty"));

    PasswordHash = newPasswordHash.Trim();
    HasPasswordSet = true;
    PasswordLastChangedAt = DateTime.UtcNow;
    return Result.Success();
  }

  public void Deactivate()
  {
    if (Deactivated)
      return;

    Deactivated = true;
    DeactivatedAt = DateTime.UtcNow;
  }

  public void Activate()
  {
    Deactivated = false;
    DeactivatedAt = null;
  }

  public void MarkEmailVerified()
  {
    if (EmailVerified)
      return;

    EmailVerified = true;
    EmailVerifiedAt = DateTime.UtcNow;
  }

  public void RecordInvitationSent()
  {
    InvitationSentAt = DateTime.UtcNow;
  }

  public Result CompleteInvitationViaExternalSignIn(string? firstName, string? lastName)
  {
    if (HasPasswordSet)
      return Result.Error("Invitation was already accepted.");

    if (InvitationSentAt is null)
      return Result.Error("No invitation is pending.");

    if (!HasCompleteProfile
        && !string.IsNullOrWhiteSpace(firstName)
        && !string.IsNullOrWhiteSpace(lastName))
    {
      var profileResult = UpdateProfile(firstName, lastName);
      if (!profileResult.IsSuccess)
        return profileResult;
    }

    InvitationSentAt = null;
    MarkEmailVerified();
    return Result.Success();
  }

  public Result EnableTwoFactor(string encryptedSecret, DateTime utcNow)
  {
    if (string.IsNullOrWhiteSpace(encryptedSecret))
      return Result.Invalid(new ValidationError(nameof(TwoFactorSecretCiphertext), "cannot be null or empty"));

    if (encryptedSecret.Length > TwoFactorConstraints.ENCRYPTED_SECRET_MAX_LENGTH)
      return Result.Invalid(new ValidationError(
        nameof(TwoFactorSecretCiphertext),
        $"cannot be longer than {TwoFactorConstraints.ENCRYPTED_SECRET_MAX_LENGTH} characters"));

    TwoFactorEnabled = true;
    TwoFactorEnabledAt = utcNow;
    TwoFactorSecretCiphertext = encryptedSecret.Trim();
    return Result.Success();
  }

  public void DisableTwoFactor()
  {
    TwoFactorEnabled = false;
    TwoFactorEnabledAt = null;
    TwoFactorSecretCiphertext = string.Empty;
    recoveryCodes.Clear();
  }

  public Result AddExternalLogin(ExternalLogin externalLogin)
  {
    if (externalLogins.Any(x =>
          x.ProviderKey.Equals(externalLogin.ProviderKey, StringComparison.OrdinalIgnoreCase)))
      return Result.Error("This provider is already linked to the account.");

    externalLogins.Add(externalLogin);
    return Result.Success();
  }

  public Result RemoveExternalLogin(string providerKey)
  {
    var login = externalLogins.FirstOrDefault(x =>
      x.ProviderKey.Equals(providerKey, StringComparison.OrdinalIgnoreCase));
    if (login is null)
      return Result.NotFound();

    externalLogins.Remove(login);
    return Result.Success();
  }

  public void ReplaceRecoveryCodes(IEnumerable<UserRecoveryCode> codes)
  {
    recoveryCodes.Clear();
    recoveryCodes.AddRange(codes);
  }

  public Result ReplaceRoles(IEnumerable<Guid> roleIds)
  {
    var distinctRoleIds = roleIds.Distinct().ToList();
    if (distinctRoleIds.Count == 0)
      return Result.Error("At least one role is required.");

    foreach (var assignment in roles.Where(x => !distinctRoleIds.Contains(x.RoleId)).ToList())
      roles.Remove(assignment);

    foreach (var roleId in distinctRoleIds)
    {
      if (!roles.Any(x => x.RoleId == roleId))
        roles.Add(UserRole.Create(Id, roleId));
    }

    return Result.Success();
  }

  public Result AssignRole(Guid roleId)
  {
    if (roles.Any(x => x.RoleId == roleId))
      return Result.Success();

    roles.Add(UserRole.Create(Id, roleId));
    return Result.Success();
  }

  public Result RemoveRole(Guid roleId)
  {
    var assignment = roles.FirstOrDefault(x => x.RoleId == roleId);
    if (assignment is null)
      return Result.NotFound();

    if (roles.Count <= 1)
      return Result.Error("Each user must have at least one role.Assign another role before removing this one.");

    roles.Remove(assignment);
    return Result.Success();
  }

  public bool HasRole(Guid roleId) => roles.Any(x => x.RoleId == roleId);

  public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

  private static List<ValidationError> ValidateWithPassword(
    string firstName,
    string lastName,
    string email,
    string passwordHash)
  {
    var validationErrors = new List<ValidationError>();
    ValidateName(firstName, nameof(FirstName), validationErrors);
    ValidateName(lastName, nameof(LastName), validationErrors);
    ValidateEmail(email, validationErrors);

    if (string.IsNullOrWhiteSpace(passwordHash))
      validationErrors.Add(new ValidationError(nameof(PasswordHash), "cannot be null or empty"));

    return validationErrors;
  }

  private static void ValidateEmail(string email, ICollection<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      validationErrors.Add(new ValidationError(nameof(Email), "cannot be null or empty"));
      return;
    }

    if (email.Trim().Length > UserConstraints.EMAIL_MAX_LENGTH)
      validationErrors.Add(new ValidationError(nameof(Email), $"cannot be longer than {UserConstraints.EMAIL_MAX_LENGTH} characters"));

    try
    {
      _ = new MailAddress(email.Trim());
    }
    catch (FormatException)
    {
      validationErrors.Add(new ValidationError(nameof(Email), "has invalid format"));
    }
  }

  private static void ValidateName(string value, string propertyName, ICollection<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      validationErrors.Add(new ValidationError(propertyName, "cannot be null or empty"));
      return;
    }

    if (value.Trim().Length > UserConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(propertyName, $"cannot be longer than {UserConstraints.NAME_MAX_LENGTH} characters"));
  }

  private static void ValidateOptionalName(
    string? value,
    string propertyName,
    ICollection<ValidationError> validationErrors)
  {
    if (string.IsNullOrWhiteSpace(value))
      return;

    if (value.Trim().Length > UserConstraints.NAME_MAX_LENGTH)
      validationErrors.Add(new ValidationError(propertyName, $"cannot be longer than {UserConstraints.NAME_MAX_LENGTH} characters"));
  }
}

public static class UserConstraints
{
  public const int NAME_MAX_LENGTH = 100;
  public const int EMAIL_MAX_LENGTH = 320;
  public const int PASSWORD_MIN_LENGTH = 8;
  public const int PASSWORD_MAX_LENGTH = 128;
}
