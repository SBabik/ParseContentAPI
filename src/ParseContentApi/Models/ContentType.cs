using System.Text.Json.Serialization;
using ParseContentApi.Json;

namespace ParseContentApi.Models;

/// <summary>
/// Supported content types carried in the "content" field of the request.
/// Adding a new format requires: a new value here, an entry in
/// <see cref="ContentTypeJsonConverter"/>, and a new implementation of
/// <see cref="Services.IContentParser"/> registered in
/// <see cref="Services.ContentParserFactory"/>.
/// </summary>
[JsonConverter(typeof(ContentTypeJsonConverter))]
public enum ContentType
{
    Csv,
    InternalJson
}
