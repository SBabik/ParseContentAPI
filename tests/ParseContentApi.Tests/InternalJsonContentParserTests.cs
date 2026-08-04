using ParseContentApi.Exceptions;
using ParseContentApi.Services;
using Xunit;

namespace ParseContentApi.Tests;

public class InternalJsonContentParserTests
{
    private readonly InternalJsonContentParser _parser = new();

    [Fact]
    public void Parse_JsonArrayOfObjects_ReturnsAllElements()
    {
        const string json = """[{"id":1,"name":"a"},{"id":2,"name":"b"}]""";

        var result = _parser.Parse(json);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Parse_SingleJsonObject_IsWrappedAsOneElement()
    {
        const string json = """{"id":1,"name":"a"}""";

        var result = _parser.Parse(json);

        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Parse_JsonArrayOfPrimitives_Throws()
    {
        const string json = "[1, 2, 3]";

        Assert.Throws<ParsingException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        const string json = "{not valid json";

        Assert.Throws<ParsingException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_EmptyContent_Throws()
    {
        Assert.Throws<ParsingException>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_EmptyArray_ReturnsZeroCount()
    {
        const string json = "[]";

        var result = _parser.Parse(json);

        Assert.Equal(0, result.Count);
    }
}
