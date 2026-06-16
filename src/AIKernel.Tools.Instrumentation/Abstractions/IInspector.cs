using AIKernel.Common.Results;

namespace AIKernel.Tools.Instrumentation.Abstractions;

/// <summary>
/// [EN] Stable inspection contract for deterministic AIKernel.Tools diagnostics.
/// [JA] deterministic AIKernel.Tools diagnostics 向けの安定した inspection contract です。
/// </summary>
public interface IInspector
{
    /// <summary>
    /// [EN] Safely inspects a value.
    /// [JA] value を安全に inspect します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to inspect.
    /// [JA] inspect 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Inspection result.
    /// [JA] inspection result です。
    /// </returns>
    Result<string> TryInspect(object? value);

    /// <summary>
    /// [EN] Safely builds a tree view for a value.
    /// [JA] value の tree view を安全に構築します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to inspect.
    /// [JA] inspect 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Tree result.
    /// [JA] tree result です。
    /// </returns>
    Result<string> TryTree(object? value);

    /// <summary>
    /// [EN] Safely computes a deterministic diff.
    /// [JA] deterministic diff を安全に計算します。
    /// </summary>
    /// <param name="left">
    /// [EN] Left value.
    /// [JA] 左側の value です。
    /// </param>
    /// <param name="right">
    /// [EN] Right value.
    /// [JA] 右側の value です。
    /// </param>
    /// <returns>
    /// [EN] Diff result.
    /// [JA] diff result です。
    /// </returns>
    Result<string> TryDiff(object? left, object? right);
}
