using System.Text.RegularExpressions;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class EmailLinkTokenExtractor
{
  public static string FromBody(string body)
  {
    var match = Regex.Match(body, @"token=([^""&]+)");
    return match.Success ? Uri.UnescapeDataString(match.Groups[1].Value) : string.Empty;
  }
}
