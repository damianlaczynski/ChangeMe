using System.Security.Cryptography;
using System.Text;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class ExternalAuthPkceUtils
{
  public static (string CodeVerifier, string CodeChallenge, string State, string Nonce) CreateAuthorizationParameters()
  {
    var codeVerifier = CreateRandomToken();
    var codeChallenge = CreateCodeChallenge(codeVerifier);
    return (codeVerifier, codeChallenge, CreateRandomToken(), CreateRandomToken());
  }

  private static string CreateCodeChallenge(string codeVerifier)
  {
    var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
    return Base64UrlEncode(challengeBytes);
  }

  private static string CreateRandomToken()
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    return Base64UrlEncode(bytes);
  }

  private static string Base64UrlEncode(byte[] bytes) =>
    Convert.ToBase64String(bytes)
      .TrimEnd('=')
      .Replace('+', '-')
      .Replace('/', '_');
}
