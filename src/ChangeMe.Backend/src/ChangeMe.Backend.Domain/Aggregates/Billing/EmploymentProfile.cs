namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class EmploymentProfile : Entity, IAggregateRoot
{
  private EmploymentProfile() { }

  public Guid UserId { get; private set; }
  public string EmployeeId { get; private set; } = string.Empty;
  public string NormalizedEmployeeId { get; private set; } = string.Empty;
  public string NationalId { get; private set; } = string.Empty;
  public string TaxId { get; private set; } = string.Empty;
  public string BankAccount { get; private set; } = string.Empty;
  public string Notes { get; private set; } = string.Empty;

  public static Result<EmploymentProfile> Create(
    Guid userId,
    string? employeeId,
    string? nationalId,
    string? taxId,
    string? bankAccount,
    string? notes)
  {
    if (userId == Guid.Empty)
      return Result.Invalid(new ValidationError(nameof(UserId), "User is required."));

    var validationErrors = ValidateFields(employeeId, nationalId, taxId, bankAccount, notes);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    var profile = new EmploymentProfile
    {
      UserId = userId,
      EmployeeId = employeeId?.Trim() ?? string.Empty,
      NormalizedEmployeeId = NormalizeEmployeeId(employeeId),
      NationalId = nationalId?.Trim() ?? string.Empty,
      TaxId = taxId?.Trim() ?? string.Empty,
      BankAccount = bankAccount?.Trim() ?? string.Empty,
      Notes = notes?.Trim() ?? string.Empty,
    };

    return Result.Success(profile);
  }

  public Result<EmploymentProfile> Update(
    string? employeeId,
    string? nationalId,
    string? taxId,
    string? bankAccount,
    string? notes)
  {
    var validationErrors = ValidateFields(employeeId, nationalId, taxId, bankAccount, notes);
    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    EmployeeId = employeeId?.Trim() ?? string.Empty;
    NormalizedEmployeeId = NormalizeEmployeeId(employeeId);
    NationalId = nationalId?.Trim() ?? string.Empty;
    TaxId = taxId?.Trim() ?? string.Empty;
    BankAccount = bankAccount?.Trim() ?? string.Empty;
    Notes = notes?.Trim() ?? string.Empty;

    return Result.Success(this);
  }

  public static string NormalizeEmployeeId(string? employeeId) =>
    string.IsNullOrWhiteSpace(employeeId) ? string.Empty : employeeId.Trim().ToUpperInvariant();

  private static List<ValidationError> ValidateFields(
    string? employeeId,
    string? nationalId,
    string? taxId,
    string? bankAccount,
    string? notes)
  {
    var validationErrors = new List<ValidationError>();

    if (employeeId is not null && employeeId.Trim().Length > BillingConstraints.EmployeeIdMaxLength)
      validationErrors.Add(new ValidationError(nameof(EmployeeId), $"cannot be longer than {BillingConstraints.EmployeeIdMaxLength} characters"));

    if (nationalId is not null && nationalId.Trim().Length > BillingConstraints.NationalIdMaxLength)
      validationErrors.Add(new ValidationError(nameof(NationalId), $"cannot be longer than {BillingConstraints.NationalIdMaxLength} characters"));

    if (taxId is not null && taxId.Trim().Length > BillingConstraints.TaxIdMaxLength)
      validationErrors.Add(new ValidationError(nameof(TaxId), $"cannot be longer than {BillingConstraints.TaxIdMaxLength} characters"));

    if (bankAccount is not null && bankAccount.Trim().Length > BillingConstraints.BankAccountMaxLength)
      validationErrors.Add(new ValidationError(nameof(BankAccount), $"cannot be longer than {BillingConstraints.BankAccountMaxLength} characters"));

    if (notes is not null && notes.Trim().Length > BillingConstraints.EmploymentNotesMaxLength)
      validationErrors.Add(new ValidationError(nameof(Notes), $"cannot be longer than {BillingConstraints.EmploymentNotesMaxLength} characters"));

    return validationErrors;
  }
}
