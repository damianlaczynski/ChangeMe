namespace ChangeMe.Backend.Domain.Interfaces;

public interface IPasswordPolicyValidator
{
  IReadOnlyList<ValidationError> Validate(string password, string propertyName = "Password");
}
