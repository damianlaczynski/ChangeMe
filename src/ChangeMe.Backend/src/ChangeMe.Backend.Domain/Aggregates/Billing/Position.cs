namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class Position : Entity, IAggregateRoot
{
  private Position() { }

  public string Name { get; private set; } = string.Empty;
  public string NormalizedName { get; private set; } = string.Empty;
  public string Department { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public bool IsActive { get; private set; } = true;

  public static Result<Position> Create(string name, string? department, string? description, bool isActive = true)
  {
    var validationErrors = ValidateName(name);
    ValidateDepartment(department, validationErrors);
    ValidateDescription(description, validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var position = new Position
    {
      Name = name.Trim(),
      NormalizedName = NormalizeName(name),
      Department = department?.Trim() ?? string.Empty,
      Description = description?.Trim() ?? string.Empty,
      IsActive = isActive,
    };

    return Result.Success(position);
  }

  public Result<Position> Update(string name, string? department, string? description, bool isActive)
  {
    var validationErrors = ValidateName(name);
    ValidateDepartment(department, validationErrors);
    ValidateDescription(description, validationErrors);

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    Name = name.Trim();
    NormalizedName = NormalizeName(name);
    Department = department?.Trim() ?? string.Empty;
    Description = description?.Trim() ?? string.Empty;
    IsActive = isActive;

    return Result.Success(this);
  }

  public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

  private static List<ValidationError> ValidateName(string name)
  {
    var validationErrors = new List<ValidationError>();
    var trimmed = name?.Trim() ?? string.Empty;

    if (trimmed.Length < BillingConstraints.PositionNameMinLength)
      validationErrors.Add(new ValidationError(nameof(Name), $"must be at least {BillingConstraints.PositionNameMinLength} characters"));
    else if (trimmed.Length > BillingConstraints.PositionNameMaxLength)
      validationErrors.Add(new ValidationError(nameof(Name), $"cannot be longer than {BillingConstraints.PositionNameMaxLength} characters"));

    return validationErrors;
  }

  private static void ValidateDepartment(string? department, ICollection<ValidationError> validationErrors)
  {
    if (department is not null && department.Trim().Length > BillingConstraints.PositionDepartmentMaxLength)
      validationErrors.Add(new ValidationError(nameof(Department), $"cannot be longer than {BillingConstraints.PositionDepartmentMaxLength} characters"));
  }

  private static void ValidateDescription(string? description, ICollection<ValidationError> validationErrors)
  {
    if (description is not null && description.Trim().Length > BillingConstraints.PositionDescriptionMaxLength)
      validationErrors.Add(new ValidationError(nameof(Description), $"cannot be longer than {BillingConstraints.PositionDescriptionMaxLength} characters"));
  }
}
