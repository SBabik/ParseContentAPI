using System.Text;

namespace ParseContentApi.Services;

/// <summary>
/// A simple CSV tokenizer with no dependencies beyond the BCL, supporting the
/// core parts of RFC 4180:
/// quoted fields may contain commas and newline characters,
/// a doubled quote "" inside a quoted field represents a literal quote,
/// row separators are \r\n and \n.
/// </summary>
internal static class CsvTokenizer
{
    public static List<List<string>> Tokenize(string content)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var index = 0;
        var length = content.Length;

        void EndField()
        {
            currentRow.Add(field.ToString());
            field.Clear();
        }

        void EndRow()
        {
            EndField();
            rows.Add(currentRow);
            currentRow = new List<string>();
        }

        while (index < length)
        {
            var current = content[index];

            if (inQuotes)
            {
                if (current == '"')
                {
                    var hasEscapedQuote = index + 1 < length && content[index + 1] == '"';
                    if (hasEscapedQuote)
                    {
                        field.Append('"');
                        index += 2;
                        continue;
                    }

                    inQuotes = false;
                    index++;
                    continue;
                }

                field.Append(current);
                index++;
                continue;
            }

            switch (current)
            {
                case '"':
                    inQuotes = true;
                    index++;
                    break;
                case ',':
                    EndField();
                    index++;
                    break;
                case '\r':
                    var hasFollowingNewLine = index + 1 < length && content[index + 1] == '\n';
                    if (hasFollowingNewLine)
                    {
                        index++;
                    }
                    EndRow();
                    index++;
                    break;
                case '\n':
                    EndRow();
                    index++;
                    break;
                default:
                    field.Append(current);
                    index++;
                    break;
            }
        }

        // Close the last row if the content did not end with a newline character.
        if (field.Length > 0 || currentRow.Count > 0)
        {
            EndRow();
        }

        // Drop a trailing, fully empty row (for example caused by a trailing newline).
        if (rows.Count > 0 && rows[^1].Count == 1 && rows[^1][0].Length == 0)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        return rows;
    }
}
