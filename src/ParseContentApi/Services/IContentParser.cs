namespace ParseContentApi.Services;

/// <summary>
/// Result of a parsing operation: the number of items and the data itself
/// as a unified structure ready to be serialized as JSON.
/// </summary>
public sealed record ParseResult(int Count, object Data);

/// <summary>
/// Contract implemented by each format specific parser.
/// A new data format means a new implementation of this interface plus a
/// registration in <see cref="ContentParserFactory"/>, without touching the
/// existing endpoint logic.
/// </summary>
public interface IContentParser
{
    /// <summary>
    /// Parses content that has already been decoded from Base64 into text.
    /// </summary>
    /// <exception cref="Exceptions.ParsingException">
    /// Thrown when the content is empty, malformed, or otherwise cannot be
    /// parsed according to the expected format.
    /// </exception>
    ParseResult Parse(string decodedContent);
}
