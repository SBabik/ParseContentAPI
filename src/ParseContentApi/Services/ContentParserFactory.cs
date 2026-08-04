using ParseContentApi.Exceptions;
using ParseContentApi.Models;

namespace ParseContentApi.Services;

/// <summary>
/// Creates the appropriate <see cref="IContentParser"/> based on <see cref="ContentType"/>.
/// Adding support for a new format comes down to adding a new switch branch
/// (plus, before that, a new enum value and an entry in the JSON converter);
/// the rest of the endpoint (Base64 decoding, response building, error
/// handling) stays unchanged.
/// </summary>
public interface IContentParserFactory
{
    IContentParser Create(ContentType type);
}

public sealed class ContentParserFactory : IContentParserFactory
{
    public IContentParser Create(ContentType type) => type switch
    {
        ContentType.Csv => new CsvContentParser(),
        ContentType.InternalJson => new InternalJsonContentParser(),
        _ => throw new ParsingException($"Unsupported content type: '{type}'.")
    };
}
