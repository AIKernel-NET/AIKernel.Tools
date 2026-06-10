using System.Collections;
using System.Globalization;
using AIKernel.Common.Results;
using AIKernel.Tools.Instrumentation.Abstractions;

namespace AIKernel.Tools.Instrumentation;

/// <summary>
/// [EN] Canonical formatter for deterministic instrumentation output.
/// [JA] deterministic instrumentation output 向けの canonical formatter です。
/// </summary>
public sealed class CanonicalFormatter : ICanonicalFormatter
{
    /// <summary>
    /// [EN] Formats a value using deterministic key and item ordering.
    /// [JA] 決定論的な key/item ordering で value を format します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to format.
    /// [JA] format 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Canonical text.
    /// [JA] canonical text です。
    /// </returns>
    public string Format(object? value)
        => RequireSuccess(TryFormat(value));

    /// <inheritdoc />
    public Result<string> TryFormat(object? value)
        => Try.Run(() => FormatValue(value));

    /// <summary>
    /// [EN] Serializes a value using the same canonical representation as Format.
    /// [JA] Format と同じ canonical representation で value を serialize します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to serialize.
    /// [JA] serialize 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Canonical serialized text.
    /// [JA] canonical serialized text です。
    /// </returns>
    public string Serialize(object? value)
        => RequireSuccess(TrySerialize(value));

    /// <inheritdoc />
    public Result<string> TrySerialize(object? value)
        => Try.Run(() => FormatValue(value));

    private static T RequireSuccess<T>(Result<T> result)
        => result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);

    private static string FormatValue(object? value)
        => value switch
        {
            null => "null",
            string text => text,
            bool boolean => FormatBoolean(boolean),
            IFormattable formattable when value is not DateTime and not DateTimeOffset =>
                formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            DateTimeOffset timestamp => timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            DateTime timestamp => timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            IDictionary dictionary => FormatDictionary(dictionary),
            IEnumerable enumerable => FormatEnumerable(enumerable),
            _ => value.ToString() ?? string.Empty
        };

    private static string FormatDictionary(IDictionary dictionary)
    {
        var entries = dictionary.Keys
            .Cast<object?>()
            .Select(key => (
                Key: FormatValue(key),
                Value: FormatValue(ReadDictionaryValue(dictionary, key).Match(() => null, item => item))))
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => $"{entry.Key}: {entry.Value}");
        return "{" + string.Join(", ", entries) + "}";
    }

    private static string FormatBoolean(bool value)
        => MonadicDecision.SelectText(value, "false", "true");

    private static Option<object?> ReadDictionaryValue(IDictionary dictionary, object? key)
        => key switch
        {
            null => Option<object?>.None(),
            _ => Option<object?>.Some(dictionary[key])
        };

    private static string FormatEnumerable(IEnumerable enumerable)
    {
        var entries = enumerable.Cast<object?>().Select(FormatValue);
        return "[" + string.Join(", ", entries) + "]";
    }
}
