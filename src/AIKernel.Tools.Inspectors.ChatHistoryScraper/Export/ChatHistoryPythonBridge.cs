using AIKernel.Providers.ChatHistory;
using System.Globalization;

namespace AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;

/// <summary>
/// [EN] Python bridge for public chat-history exporters.
/// [JA] 公開 chat-history exporter 用の Python bridge です。
/// </summary>
public static class ChatHistoryPythonBridge
{
    /// <summary>
    /// [EN] Formats chat-history records as Markdown through MdExporter.
    /// [JA] MdExporter 経由で chat-history record を Markdown 形式にします。
    /// </summary>
    public static string ToMarkdown(
        IReadOnlyList<string> roles,
        IReadOnlyList<string> contents,
        IReadOnlyList<string> timestamps)
        => MdExporter.ToMarkdown(ToRecords(roles, contents, timestamps));

    /// <summary>
    /// [EN] Serializes chat-history records as ROM Markdown through RomExporter.
    /// [JA] RomExporter 経由で chat-history record を ROM Markdown に serialize します。
    /// </summary>
    public static string ToRom(
        IReadOnlyList<string> roles,
        IReadOnlyList<string> contents,
        IReadOnlyList<string> timestamps)
        => RomExporter.ToRom(ToRecords(roles, contents, timestamps));

    /// <summary>
    /// [EN] Serializes chat-history records as ROM Markdown through RomExporter with explicit metadata.
    /// [JA] 明示的な metadata を指定して RomExporter 経由で chat-history record を ROM Markdown に serialize します。
    /// </summary>
    public static string ToRomWithMetadata(
        IReadOnlyList<string> roles,
        IReadOnlyList<string> contents,
        IReadOnlyList<string> timestamps,
        string @namespace,
        string name,
        string? generatedAtUtc)
        => RomExporter.ToRom(
            ToRecords(roles, contents, timestamps),
            @namespace,
            name,
            string.IsNullOrWhiteSpace(generatedAtUtc)
                ? null
                : DateTimeOffset.Parse(generatedAtUtc, CultureInfo.InvariantCulture));

    private static IReadOnlyList<ChatHistoryRecord> ToRecords(
        IReadOnlyList<string> roles,
        IReadOnlyList<string> contents,
        IReadOnlyList<string> timestamps)
    {
        if (roles.Count != contents.Count || roles.Count != timestamps.Count)
        {
            throw new ArgumentException("Chat history roles, contents, and timestamps must have the same count.");
        }

        var records = new List<ChatHistoryRecord>(roles.Count);
        for (var i = 0; i < roles.Count; i++)
        {
            records.Add(new ChatHistoryRecord
            {
                Role = roles[i],
                Content = contents[i],
                Timestamp = DateTimeOffset.Parse(timestamps[i])
            });
        }

        return records;
    }
}
