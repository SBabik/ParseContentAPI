using System.Text.Json;
using ParseContentApi.Exceptions;

namespace ParseContentApi.Services;

/// <summary>
/// Validates and deserializes the internal JSON format.
/// Accepted shapes of the decoded content:
///   an array of JSON objects, such as [{...}, {...}], where Count equals the array length,
///   a single JSON object, such as {...}, treated as a one element array (Count equals 1).
/// Any other shape (number, string, array of primitives, null, invalid JSON)
/// is rejected as a validation error (400).
/// </summary>
public sealed class InternalJsonContentParser : IContentParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ParseResult Parse(string decodedContent)
    {
        if (string.IsNullOrWhiteSpace(decodedContent))
        {
            throw new ParsingException("The decoded JSON content is empty.");
        }

        using JsonDocument document = ParseDocument(decodedContent);

        var root = document.RootElement;

        List<JsonElement> elements = root.ValueKind switch
        {
            JsonValueKind.Array => root.EnumerateArray().ToList(),
            JsonValueKind.Object => new List<JsonElement> { root },
            _ => throw new ParsingException(
                "Invalid INTERNAL_JSON structure: expected a JSON object or an array of JSON objects.")
        };

        ValidateElementsAreObjects(elements);

        var data = elements
            .Select(element => JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText(), SerializerOptions))
            .ToList();

        return new ParseResult(data.Count, data);
    }

    private static JsonDocument ParseDocument(string decodedContent)
    {
        try
        {
            return JsonDocument.Parse(decodedContent);
        }
        catch (JsonException ex)
        {
            throw new ParsingException($"Invalid JSON in field 'content': {ex.Message}");
        }
    }

    private static void ValidateElementsAreObjects(List<JsonElement> elements)
    {
        for (var i = 0; i < elements.Count; i++)
        {
            if (elements[i].ValueKind != JsonValueKind.Object)
            {
                throw new ParsingException(
                    $"Element at index {i} is not a JSON object (received: {elements[i].ValueKind}).");
            }
        }
    }
}
