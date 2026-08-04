using System.Text.Json;
using System.Text.Json.Serialization;
using ParseContentApi.Models;

namespace ParseContentApi.Json;

/// <summary>
/// Maps the string values used in the payload ("CSV", "INTERNAL_JSON") to the
/// <see cref="ContentType"/> enum. An unknown or missing value results in a
/// <see cref="JsonException"/> being thrown, which ASP.NET Core catches during
/// body binding and, thanks to the middleware registered in Program.cs, turns
/// into a 400 Bad Request with a readable error message.
/// </summary>
public sealed class ContentTypeJsonConverter : JsonConverter<ContentType>
{
    private const string CsvToken = "CSV";
    private const string InternalJsonToken = "INTERNAL_JSON";

    public override ContentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Field 'type' must be a string. Allowed values: {CsvToken}, {InternalJsonToken}.");
        }

        var rawValue = reader.GetString();

        return rawValue?.Trim().ToUpperInvariant() switch
        {
            CsvToken => ContentType.Csv,
            InternalJsonToken => ContentType.InternalJson,
            _ => throw new JsonException(
                $"Unsupported content type: '{rawValue}'. Allowed values: {CsvToken}, {InternalJsonToken}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, ContentType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            ContentType.Csv => CsvToken,
            ContentType.InternalJson => InternalJsonToken,
            _ => value.ToString()
        });
    }
}
