namespace AIKernel.Tools.Instrumentation;

using AIKernel.Common.Results;
using AIKernel.Tools.Instrumentation.Abstractions;

/// <summary>
/// [EN] Deterministic inspector facade for public AIKernel.Tools diagnostics.
/// [JA] public AIKernel.Tools diagnostics 向けの deterministic inspector facade です。
/// </summary>
public sealed class Inspector : IInspector
{
    private readonly CanonicalFormatter _formatter = new();

    /// <summary>
    /// [EN] Returns a compact inspection summary for a value.
    /// [JA] value の compact inspection summary を返します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to inspect.
    /// [JA] inspect 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Inspection summary.
    /// [JA] inspection summary です。
    /// </returns>
    public string Inspect(object? value)
        => RequireSuccess(TryInspect(value));

    /// <inheritdoc />
    public Result<string> TryInspect(object? value)
        =>
            from formatted in _formatter.TryFormat(value)
            select value is null
                ? "type: null\nvalue: null"
                : $"type: {value.GetType().FullName}\nvalue: {formatted}";

    /// <summary>
    /// [EN] Returns a tree-compatible canonical view for a value.
    /// [JA] value の tree-compatible canonical view を返します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to inspect.
    /// [JA] inspect 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Tree view text.
    /// [JA] tree view text です。
    /// </returns>
    public string Tree(object? value)
        => RequireSuccess(TryTree(value));

    /// <inheritdoc />
    public Result<string> TryTree(object? value)
        => _formatter.TryFormat(value);

    /// <summary>
    /// [EN] Returns a deterministic text diff summary between two values.
    /// [JA] 2 つの value 間の deterministic text diff summary を返します。
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
    /// [EN] Diff summary.
    /// [JA] diff summary です。
    /// </returns>
    public string Diff(object? left, object? right)
        => RequireSuccess(TryDiff(left, right));

    /// <inheritdoc />
    public Result<string> TryDiff(object? left, object? right)
        =>
            from leftText in _formatter.TryFormat(left)
            from rightText in _formatter.TryFormat(right)
            select string.Equals(leftText, rightText, StringComparison.Ordinal)
                ? "equal: true"
                : $"equal: false\nleft: {leftText}\nright: {rightText}";

    private static T RequireSuccess<T>(Result<T> result)
        => result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);
}
