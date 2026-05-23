using System.Security.Cryptography;
using System.Text;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class SecureTokenGenerator
{
  public static string CreateToken()
  {
    var bytes = RandomNumberGenerator.GetBytes(UserAuthTokenConstraints.TOKEN_BYTES);
    return Convert.ToBase64String(bytes);
  }

  public static string HashToken(string plainToken)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
    return Convert.ToHexString(bytes);
  }
}
