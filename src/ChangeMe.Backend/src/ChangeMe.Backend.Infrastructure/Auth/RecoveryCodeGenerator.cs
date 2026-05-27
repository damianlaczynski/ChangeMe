using System.Security.Cryptography;
using System.Text;

namespace ChangeMe.Backend.Infrastructure.Auth;

public static class RecoveryCodeGenerator
{
  private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

  public static IReadOnlyList<string> GenerateCodes(int count, int length)
  {
    var codes = new string[count];
    for (var i = 0; i < count; i++)
      codes[i] = GenerateCode(length);

    return codes;
  }

  private static string GenerateCode(int length)
  {
    var builder = new StringBuilder(length);
    var bytes = RandomNumberGenerator.GetBytes(length);

    for (var i = 0; i < length; i++)
      builder.Append(Alphabet[bytes[i] % Alphabet.Length]);

    return builder.ToString();
  }
}
