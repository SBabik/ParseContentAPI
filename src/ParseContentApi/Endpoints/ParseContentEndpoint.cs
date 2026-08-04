using System.Text;
using ParseContentApi.Exceptions;
using ParseContentApi.Models;
using ParseContentApi.Services;

namespace ParseContentApi.Endpoints;

/// <summary>
/// Registers POST /api/v1/parse-content.
/// </summary>
public static class ParseContentEndpoint
{
    public static IEndpointRouteBuilder MapParseContentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/parse-content", Handle)
            .WithName("ParseContent")
            .WithSummary("Decodes and parses a CSV or INTERNAL_JSON payload encoded as Base64.")
            .Accepts<ParseRequest>("application/json")
            .Produces<ParseResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static IResult Handle(ParseRequest request, IContentParserFactory parserFactory)
    {
        if (request.Type is null)
        {
            return Results.BadRequest(
                new ErrorResponse("Field 'type' is required. Allowed values: CSV, INTERNAL_JSON."));
        }

        if (string.IsNullOrEmpty(request.Content))
        {
            return Results.BadRequest(new ErrorResponse("Field 'content' is required and cannot be empty."));
        }

        string decodedContent;
        try
        {
            var bytes = Convert.FromBase64String(request.Content);
            decodedContent = Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            return Results.BadRequest(
                new ErrorResponse("Field 'content' does not contain validly encoded Base64 data."));
        }

        try
        {
            var parser = parserFactory.Create(request.Type.Value);
            var result = parser.Parse(decodedContent);

            return Results.Ok(new ParseResponse
            {
                Status = "success",
                Type = request.Type.Value,
                Count = result.Count,
                Data = result.Data
            });
        }
        catch (ParsingException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
