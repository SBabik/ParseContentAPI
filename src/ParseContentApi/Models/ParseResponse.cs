namespace ParseContentApi.Models;

/// <summary>
/// Unified response returned after the request has been processed successfully.
/// </summary>
public sealed class ParseResponse
{
    /// <summary>Always "success" for a 200 OK response.</summary>
    public string Status { get; init; } = "success";

    /// <summary>The content type that was processed.</summary>
    public required ContentType Type { get; init; }

    /// <summary>Number of processed rows (CSV) or objects (INTERNAL_JSON).</summary>
    public required int Count { get; init; }

    /// <summary>Parsed data as a collection of objects.</summary>
    public required object Data { get; init; }
}

/// <summary>
/// Unified error response. Used for every validation and parsing error
/// (400 Bad Request) as well as unexpected exceptions (500).
/// </summary>
public sealed class ErrorResponse
{
    public string Status { get; init; } = "error";

    public string Message { get; init; }

    public ErrorResponse(string message)
    {
        Message = message;
    }
}
