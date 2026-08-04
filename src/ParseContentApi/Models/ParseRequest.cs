namespace ParseContentApi.Models;

/// <summary>
/// Body of the POST /api/v1/parse-content request.
/// </summary>
/// <param name="Type">
/// Content type of the <see cref="Content"/> field (CSV or INTERNAL_JSON).
/// Intentionally nullable: when the "type" field is absent from the JSON body,
/// System.Text.Json never calls the converter and simply leaves the value at
/// its default. For a nullable type that default is null, which lets the
/// handler explicitly detect and report a missing field (instead of silently
/// treating it as the enum's zero value, CSV).
/// </param>
/// <param name="Content">Raw data encoded as Base64.</param>
public record ParseRequest(ContentType? Type, string? Content);
