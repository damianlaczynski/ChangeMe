namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class LeaveType : Entity, IAggregateRoot
{
  private LeaveType() { }

  public string Name { get; private set; } = string.Empty;
  public string NormalizedName { get; private set; } = string.Empty;
  public string Code { get; private set; } = string.Empty;
  public string NormalizedCode { get; private set; } = string.Empty;
  public bool CountsAsPaid { get; private set; }
  public bool UsesAllowance { get; private set; }
  public bool RequiresApproval { get; private set; }
  public bool IsActive { get; private set; } = true;
  public bool IsSeeded { get; private set; }

  public static LeaveType CreateSeeded(
    string name,
    string code,
    bool countsAsPaid,
    bool usesAllowance,
    bool requiresApproval)
  {
    var leaveType = new LeaveType
    {
      Name = name,
      NormalizedName = NormalizeName(name),
      Code = code.Trim().ToUpperInvariant(),
      NormalizedCode = NormalizeCode(code),
      CountsAsPaid = countsAsPaid,
      UsesAllowance = usesAllowance,
      RequiresApproval = requiresApproval,
      IsActive = true,
      IsSeeded = true,
      CreatedBy = Guid.Empty,
      UpdatedBy = Guid.Empty,
    };

    return leaveType;
  }

  public static Result<LeaveType> Create(
    string name,
    string code,
    bool countsAsPaid,
    bool usesAllowance,
    bool requiresApproval,
    bool isActive = true)
  {
    var validationErrors = Validate(name, code);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var leaveType = new LeaveType
    {
      Name = name.Trim(),
      NormalizedName = NormalizeName(name),
      Code = code.Trim().ToUpperInvariant(),
      NormalizedCode = NormalizeCode(code),
      CountsAsPaid = countsAsPaid,
      UsesAllowance = usesAllowance,
      RequiresApproval = requiresApproval,
      IsActive = isActive,
      IsSeeded = false,
    };

    return Result.Success(leaveType);
  }

  public Result<LeaveType> Update(
    string name,
    string code,
    bool countsAsPaid,
    bool usesAllowance,
    bool requiresApproval,
    bool isActive)
  {
    var validationErrors = Validate(name, code);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    Name = name.Trim();
    NormalizedName = NormalizeName(name);
    Code = code.Trim().ToUpperInvariant();
    NormalizedCode = NormalizeCode(code);
    CountsAsPaid = countsAsPaid;
    UsesAllowance = usesAllowance;
    RequiresApproval = requiresApproval;
    IsActive = isActive;

    return Result.Success(this);
  }

  public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

  public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

  private static List<ValidationError> Validate(string name, string code)
  {
    var validationErrors = new List<ValidationError>();
    var trimmedName = name?.Trim() ?? string.Empty;
    var trimmedCode = code?.Trim() ?? string.Empty;

    if (trimmedName.Length < BillingConstraints.LeaveTypeNameMinLength)
      validationErrors.Add(new ValidationError(nameof(Name), $"must be at least {BillingConstraints.LeaveTypeNameMinLength} characters"));
    else if (trimmedName.Length > BillingConstraints.LeaveTypeNameMaxLength)
      validationErrors.Add(new ValidationError(nameof(Name), $"cannot be longer than {BillingConstraints.LeaveTypeNameMaxLength} characters"));

    if (trimmedCode.Length < BillingConstraints.LeaveTypeCodeMinLength)
      validationErrors.Add(new ValidationError(nameof(Code), $"must be at least {BillingConstraints.LeaveTypeCodeMinLength} characters"));
    else if (trimmedCode.Length > BillingConstraints.LeaveTypeCodeMaxLength)
      validationErrors.Add(new ValidationError(nameof(Code), $"cannot be longer than {BillingConstraints.LeaveTypeCodeMaxLength} characters"));

    return validationErrors;
  }
}
