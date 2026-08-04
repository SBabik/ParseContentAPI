using Microsoft.AspNetCore.Diagnostics;
using ParseContentApi.Endpoints;
using ParseContentApi.Json;
using ParseContentApi.Models;
using ParseContentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

// Register the enum converter so it applies both when binding the request
// body ([FromBody]) and during any manual (de)serialization elsewhere.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new ContentTypeJsonConverter());
});

builder.Services.AddSingleton<IContentParserFactory, ContentParserFactory>();

var app = builder.Build();

// Global exception handling:
// BadHttpRequestException (thrown, among others, by ContentTypeJsonConverter,
// or when the request body is not valid JSON at all) becomes a 400 with a
// readable message. Everything else we did not anticipate becomes a 500
// with no leaked implementation details.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var (statusCode, message) = exception switch
        {
            BadHttpRequestException badRequest => (
                StatusCodes.Status400BadRequest,
                badRequest.InnerException?.Message
                    ?? "Invalid request: could not read the JSON body."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected server error occurred.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ErrorResponse(message));
    });
});

app.MapParseContentEndpoint();

app.Run();

// Allows the Program class to be used in integration tests (WebApplicationFactory<Program>).
public partial class Program
{
}
