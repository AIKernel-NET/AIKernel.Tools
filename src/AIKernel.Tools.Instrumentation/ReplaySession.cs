namespace AIKernel.Tools.Instrumentation;

/// <summary>
/// [EN] Immutable replay session snapshot exposed by AIKernel.Tools instrumentation.
/// [JA] AIKernel.Tools instrumentation が公開する immutable replay session snapshot です。
/// </summary>
public sealed class ReplaySession
{
    /// <summary>
    /// [EN] Creates a replay session snapshot.
    /// [JA] replay session snapshot を作成します。
    /// </summary>
    /// <param name="events">
    /// [EN] Ordered replay events.
    /// [JA] 順序付き replay event です。
    /// </param>
    /// <param name="state">
    /// [EN] Current replay state.
    /// [JA] 現在の replay state です。
    /// </param>
    /// <param name="metadata">
    /// [EN] Deterministic session metadata.
    /// [JA] 決定論的な session metadata です。
    /// </param>
    public ReplaySession(
        IReadOnlyList<string> events,
        string state,
        IReadOnlyDictionary<string, string> metadata)
    {
        Events = events.ToArray();
        State = state;
        Metadata = metadata
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// [EN] Ordered replay events.
    /// [JA] 順序付き replay event です。
    /// </summary>
    public IReadOnlyList<string> Events { get; }

    /// <summary>
    /// [EN] Current replay state.
    /// [JA] 現在の replay state です。
    /// </summary>
    public string State { get; }

    /// <summary>
    /// [EN] Deterministic session metadata.
    /// [JA] 決定論的な session metadata です。
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
