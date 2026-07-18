using System.Security.Cryptography;
using System.Text;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class InvitationTokenGenerator
{
  private const int TokenBytes = 32;

  public static string CreateToken()
  {
    var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
    return Convert.ToBase64String(bytes)
      .TrimEnd('=')
      .Replace('+', '-')
      .Replace('/', '_');
  }

  public static string HashToken(string token)
  {
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToHexString(bytes);
  }
}
