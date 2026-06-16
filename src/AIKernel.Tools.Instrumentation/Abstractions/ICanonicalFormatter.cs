using AIKernel.Common.Results;

namespace AIKernel.Tools.Instrumentation.Abstractions;

/// <summary>
/// [EN] Stable canonical formatting contract for AIKernel.Tools instrumentation.
/// [JA] AIKernel.Tools instrumentation 向けの安定した canonical formatting contract です。
/// </summary>
public interface ICanonicalFormatter
{
    /// <summary>
    /// [EN] Safely formats a value into deterministic text.
    /// [JA] value を deterministic text へ安全に format します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to format.
    /// [JA] format 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Formatting result.
    /// [JA] formatting result です。
    /// </returns>
    Result<string> TryFormat(object? value);

    /// <summary>
    /// [EN] Safely serializes a value into canonical text.
    /// [JA] value を canonical text へ安全に serialize します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to serialize.
    /// [JA] serialize 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Serialization result.
    /// [JA] serialization result です。
    /// </returns>
    Result<string> TrySerialize(object? value);
}
