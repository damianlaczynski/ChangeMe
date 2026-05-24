using System.Text.Json;
using System.Text.Json.Serialization;
using Fido2NetLib.Objects;

namespace ChangeMe.Backend.Infrastructure.Auth.Json;

/// <summary>
/// WebAuthn sends <c>type: "public-key"</c>; <see cref="JsonStringEnumConverter"/> expects C# enum member names.
/// </summary>
public sealed class PublicKeyCredentialTypeJsonConverter : JsonConverter<PublicKeyCredentialType>
{
  public override PublicKeyCredentialType Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options)
  {
    if (reader.TokenType != JsonTokenType.String)
    {
      throw new JsonException(
        $"Unexpected token parsing {nameof(PublicKeyCredentialType)}: {reader.TokenType}.");
    }

    var value = reader.GetString();
    return value switch
    {
      null => throw new JsonException($"{nameof(PublicKeyCredentialType)} value cannot be null."),
      "public-key" => PublicKeyCredentialType.PublicKey,
      _ when Enum.TryParse<PublicKeyCredentialType>(value, ignoreCase: true, out var parsed) => parsed,
      _ => throw new JsonException($"Unknown {nameof(PublicKeyCredentialType)}: {value}")
    };
  }

  public override void Write(
    Utf8JsonWriter writer,
    PublicKeyCredentialType value,
    JsonSerializerOptions options)
  {
    var asString = value == PublicKeyCredentialType.PublicKey ? "public-key" : value.ToString();
    writer.WriteStringValue(asString);
  }
}
