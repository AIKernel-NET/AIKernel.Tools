using AIKernel.Core.ChatHistory;
using AIKernel.Tools.CapabilityModules.ChatHistoryCapability;

namespace AIKernel.Tools.ChatHistoryScraper.Export;

public static class RomExporter
{
    public static string ToRom(IReadOnlyList<ChatHistoryRecord> records)
        => ToRom(records, "scraper", "history", generatedAtUtc: null);

    public static string ToRom(
        IReadOnlyList<ChatHistoryRecord> records,
        string @namespace,
        string name,
        DateTimeOffset? generatedAtUtc = null)
    {
        var generatedAt = generatedAtUtc ?? InferGeneratedAt(records);
        var romId = HistoryRomPath.CreateRomId(@namespace, name);
        if (romId.IsFailure)
        {
            throw new InvalidOperationException(romId.Error!.Message);
        }

        var result = ChatHistoryRomExporter.ToRomMarkdown(
            records.Select(record => new ChatHistoryRomRecord(
                    record.Role,
                    record.Content,
                    record.Timestamp))
                .ToArray(),
            new ChatHistoryRomOptions(
                romId.Value!,
                generatedAt,
                SecurityTags: ["chat", "history", "scraped"]));

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error!.Message);
        }

        return result.Value!;
    }

    private static DateTimeOffset InferGeneratedAt(
        IReadOnlyList<ChatHistoryRecord> records)
        => records.Count == 0
            ? DateTimeOffset.UnixEpoch
            : records
                .Select(record => record.Timestamp.ToUniversalTime())
                .OrderByDescending(timestamp => timestamp)
                .First();
}
