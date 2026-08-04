using System.Globalization;

namespace ParseContentApi.Services;

/// <summary>
/// Guesses the type of a single CSV cell so the returned JSON contains
/// numbers and booleans as native JSON types instead of everything being a
/// string. Order of attempts: empty becomes null, whole number becomes long,
/// decimal number becomes double, true/false becomes bool, otherwise the
/// original string is returned unchanged.
/// </summary>
internal static class CsvValueConverter
{
    public static object? Infer(string rawValue)
    {
        if (string.IsNullOrEmpty(rawValue))
        {
            return null;
        }

        if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue;
        }

        if (bool.TryParse(rawValue, out var boolValue))
        {
            return boolValue;
        }

        return rawValue;
    }
}
