using ParseContentApi.Exceptions;

namespace ParseContentApi.Services;

/// <summary>
/// Parses CSV text (first row is the column header) into a collection of
/// Dictionary&lt;string, object?&gt; objects, where the key is the column
/// name and the value is matched to the closest simple type
/// (see <see cref="CsvValueConverter"/>).
/// </summary>
public sealed class CsvContentParser : IContentParser
{
    public ParseResult Parse(string decodedContent)
    {
        if (string.IsNullOrWhiteSpace(decodedContent))
        {
            throw new ParsingException("The decoded CSV content is empty.");
        }

        var rows = CsvTokenizer.Tokenize(decodedContent);

        if (rows.Count == 0)
        {
            throw new ParsingException("Could not read any rows from the CSV data.");
        }

        var headers = rows[0];
        ValidateHeaders(headers);

        var records = new List<Dictionary<string, object?>>();

        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            // Skip fully empty rows (for example an extra blank line at the end of the file).
            if (row.Count == 1 && row[0].Length == 0)
            {
                continue;
            }

            if (row.Count != headers.Count)
            {
                // Row number in the message counted "for humans": +1 for the header row, +1 because indices start at 1.
                throw new ParsingException(
                    $"Row {rowIndex + 1} has {row.Count} columns, expected {headers.Count} based on the header.");
            }

            var record = new Dictionary<string, object?>(headers.Count);
            for (var column = 0; column < headers.Count; column++)
            {
                record[headers[column]] = CsvValueConverter.Infer(row[column]);
            }

            records.Add(record);
        }

        return new ParseResult(records.Count, records);
    }

    private static void ValidateHeaders(List<string> headers)
    {
        if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
        {
            throw new ParsingException("The CSV header is empty or invalid.");
        }

        var duplicate = headers
            .GroupBy(h => h)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
        {
            throw new ParsingException($"The CSV header contains a duplicate column: '{duplicate.Key}'.");
        }
    }
}
