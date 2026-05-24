using System.Security.Cryptography;
using System.Text;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class RecoveryCodeHasher : IRecoveryCodeHasher
{
  public string Hash(string recoveryCode)
  {
    var normalized = Normalize(recoveryCode);
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
    return Convert.ToHexString(bytes);
  }

  public bool Verify(string recoveryCode, string codeHash)
  {
    if (string.IsNullOrWhiteSpace(codeHash))
      return false;

    var computed = Hash(recoveryCode);
    return CryptographicOperations.FixedTimeEquals(
      Encoding.UTF8.GetBytes(computed),
      Encoding.UTF8.GetBytes(codeHash));
  }

  private static string Normalize(string recoveryCode) =>
    recoveryCode.Trim().ToUpperInvariant();
}
