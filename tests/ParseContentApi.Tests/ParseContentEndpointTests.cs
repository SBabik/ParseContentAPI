using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ParseContentApi.Tests;

public class ParseContentEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ParseContentEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static string ToBase64(string plainText) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    [Fact]
    public async Task PostParseContent_ValidCsv_Returns200WithParsedRows()
    {
        var payload = new
        {
            type = "CSV",
            content = ToBase64("name,age\nAlice,30\nBob,25")
        };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("success", body.GetProperty("status").GetString());
        Assert.Equal(2, body.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostParseContent_ValidInternalJson_Returns200WithParsedObjects()
    {
        var payload = new
        {
            type = "INTERNAL_JSON",
            content = ToBase64("""[{"id":1},{"id":2},{"id":3}]""")
        };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task PostParseContent_UnsupportedType_Returns400()
    {
        var payload = new { type = "XML", content = ToBase64("<a></a>") };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostParseContent_MissingType_Returns400()
    {
        var payload = new { content = ToBase64("a,b\n1,2") };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostParseContent_InvalidBase64_Returns400()
    {
        var payload = new { type = "CSV", content = "not-valid-base64-!!!" };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostParseContent_MissingContent_Returns400()
    {
        var payload = new { type = "CSV" };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostParseContent_MalformedCsvRow_Returns400()
    {
        var payload = new
        {
            type = "CSV",
            content = ToBase64("a,b,c\n1,2")
        };

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
