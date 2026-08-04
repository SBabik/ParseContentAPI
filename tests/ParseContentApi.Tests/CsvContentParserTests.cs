using ParseContentApi.Exceptions;
using ParseContentApi.Services;
using Xunit;

namespace ParseContentApi.Tests;

public class CsvContentParserTests
{
    private readonly CsvContentParser _parser = new();

    [Fact]
    public void Parse_SimpleCsv_ReturnsExpectedRecords()
    {
        const string csv = "name,age,active\nAlice,30,true\nBob,25,false";

        var result = _parser.Parse(csv);

        Assert.Equal(2, result.Count);
        var rows = Assert.IsType<List<Dictionary<string, object?>>>(result.Data);

        Assert.Equal("Alice", rows[0]["name"]);
        Assert.Equal(30L, rows[0]["age"]);
        Assert.Equal(true, rows[0]["active"]);

        Assert.Equal("Bob", rows[1]["name"]);
        Assert.Equal(25L, rows[1]["age"]);
        Assert.Equal(false, rows[1]["active"]);
    }

    [Fact]
    public void Parse_QuotedFieldWithCommaAndEscapedQuote_IsHandledCorrectly()
    {
        const string csv = "name,note\n\"Smith, John\",\"He said \"\"hi\"\"\"";

        var result = _parser.Parse(csv);

        var rows = Assert.IsType<List<Dictionary<string, object?>>>(result.Data);
        Assert.Single(rows);
        Assert.Equal("Smith, John", rows[0]["name"]);
        Assert.Equal("He said \"hi\"", rows[0]["note"]);
    }

    [Fact]
    public void Parse_TrailingBlankLine_IsIgnored()
    {
        const string csv = "a,b\n1,2\n";

        var result = _parser.Parse(csv);

        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void Parse_RowWithWrongColumnCount_Throws()
    {
        const string csv = "a,b,c\n1,2";

        var ex = Assert.Throws<ParsingException>(() => _parser.Parse(csv));
        Assert.Contains("columns", ex.Message);
    }

    [Fact]
    public void Parse_DuplicateHeader_Throws()
    {
        const string csv = "a,a\n1,2";

        Assert.Throws<ParsingException>(() => _parser.Parse(csv));
    }

    [Fact]
    public void Parse_EmptyContent_Throws()
    {
        Assert.Throws<ParsingException>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_EmptyCell_MapsToNull()
    {
        const string csv = "a,b\n1,";

        var result = _parser.Parse(csv);
        var rows = Assert.IsType<List<Dictionary<string, object?>>>(result.Data);

        Assert.Null(rows[0]["b"]);
    }
}
