namespace ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

public interface ITotpService
{
  string GenerateSecret();

  string BuildProvisioningUri(string secret, string accountName);

  bool ValidateCode(string secret, string code, DateTime utcNow);
}
