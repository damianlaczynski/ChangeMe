using System.Net.Mail;

namespace ChangeMe.Backend.Domain.Aggregates.Users;

public class User : Entity, IAggregateRoot
{
  private readonly List<UserRole> roles = [];

  private User() { }

  public IReadOnlyCollection<UserRole> Roles => roles;

  public string FirstName { get; private set; } = string.Empty;
  public string LastName { get; private set; } = string.Empty;
  public string Email { get; private set; } = string.Empty;
  public string NormalizedEmail { get; private set; } = string.Empty;
  public string PasswordHash { get; private set; } = string.Empty;
  public UserStatus Status { get; private set; } = UserStatus.Active;
  public string FullName => $"{FirstName} {LastName}";
  public bool IsActive => Status == UserStatus.Active;

  public static Result<User> Create(string firstName, string lastName, string email, string passwordHash)
  {
    var validationErrors = Validate(firstName, lastName, email, passwordHash);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var normalizedEmail = NormalizeEmail(email);
    var user = new User
    {
      FirstName = firstName.Trim(),
      LastName = lastName.Trim(),
      Email = email.Trim(),
      NormalizedEmail = normalizedEmail,
      PasswordHash = passwordHash.Trim()
    };

    user.Status = UserStatus.Active;

    return Result.Success(user);
  }

  public static Result<User> CreateInitialAdministrator(string firstName, string lastName, string email, string passwordHash)
  {
    var result = Create(firstName, lastName, email, passwordHash);
    if (!result.IsSuccess)
      return result;

    var user = result.Value;

    user.CreatedBy = Guid.Empty;
    user.UpdatedBy = Guid.Empty;

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
    ValidateName(firstName, nameof(FirstName), validationErrors);
    ValidateName(lastName, nameof(LastName), validationErrors);

    if (string.IsNullOrWhiteSpace(email))
    {
      validationErrors.Add(new ValidationError(nameof(Email), "cannot be null or empty"));
    }
    else
    {
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

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    FirstName = firstName.Trim();
    LastName = lastName.Trim();
    Email = email.Trim();
    NormalizedEmail = NormalizeEmail(email);
    return Result.Success();
  }

  public Result SetPasswordHash(string newPasswordHash)
  {
    if (string.IsNullOrWhiteSpace(newPasswordHash))
      return Result.Invalid(new ValidationError(nameof(PasswordHash), "cannot be null or empty"));

    PasswordHash = newPasswordHash.Trim();
    return Result.Success();
  }

  public void Deactivate() => Status = UserStatus.Inactive;

  public void Activate() => Status = UserStatus.Active;

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

  private static List<ValidationError> Validate(string firstName, string lastName, string email, string passwordHash)
  {
    var validationErrors = new List<ValidationError>();

    ValidateName(firstName, nameof(FirstName), validationErrors);
    ValidateName(lastName, nameof(LastName), validationErrors);

    if (string.IsNullOrWhiteSpace(email))
    {
      validationErrors.Add(new ValidationError(nameof(Email), "cannot be null or empty"));
    }
    else
    {
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

    if (string.IsNullOrWhiteSpace(passwordHash))
      validationErrors.Add(new ValidationError(nameof(PasswordHash), "cannot be null or empty"));

    return validationErrors;
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
}

public static class UserConstraints
{
  public const int NAME_MAX_LENGTH = 100;
  public const int EMAIL_MAX_LENGTH = 320;
  public const int PASSWORD_MIN_LENGTH = 8;
  public const int PASSWORD_MAX_LENGTH = 128;
}
