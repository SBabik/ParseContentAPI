namespace ParseContentApi.Exceptions;

/// <summary>
/// Thrown when the decoded content does not match the expected format
/// (for example invalid CSV, invalid JSON, or an unsupported type).
/// The endpoint catches this exception and returns a 400 Bad Request with
/// the exception message, unlike unexpected exceptions which result in a 500.
/// </summary>
public sealed class ParsingException : Exception
{
    public ParsingException(string message) : base(message)
    {
    }
}
