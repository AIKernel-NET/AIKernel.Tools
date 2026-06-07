using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AIKernel.Tools.Capability.RomStorage.ChatHistory;

namespace AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;

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
        var romId = CreateRomId(@namespace, name);
        var body = BuildBody(records);
        var hash = ComputeSha256($"{romId}\n{generatedAt:O}\n{body}");

        return BuildMarkdown(
            romId,
            generatedAt,
            ["chat", "history", "scraped"],
            hash,
            body);
    }

    private static DateTimeOffset InferGeneratedAt(
        IReadOnlyList<ChatHistoryRecord> records)
        => records.Count == 0
            ? DateTimeOffset.UnixEpoch
            : records
                .Select(record => record.Timestamp.ToUniversalTime())
                .OrderByDescending(timestamp => timestamp)
                .First();

    private static string CreateRomId(
        string @namespace,
        string name)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            throw new ArgumentException("History ROM namespace is required.", nameof(@namespace));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("History ROM name is required.", nameof(name));
        }

        return $"history://{SanitizeSegment(@namespace)}/{SanitizeSegment(name)}";
    }

    private static string BuildBody(
        IReadOnlyList<ChatHistoryRecord> records)
    {
        if (records.Count == 0)
        {
            throw new InvalidOperationException("Chat history records are required.");
        }

        var builder = new StringBuilder();
        builder.AppendLine("# ROM:ChatHistory");
        builder.AppendLine();

        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (string.IsNullOrWhiteSpace(record.Role))
            {
                throw new InvalidOperationException($"Chat history record role is required. Index='{i}'.");
            }

            if (record.Content is null)
            {
                throw new InvalidOperationException($"Chat history record content is required. Index='{i}'.");
            }

            builder.AppendLine($"## Turn:{i + 1}");
            builder.AppendLine($"@role: {record.Role.Trim()}");
            builder.AppendLine(
                $"@time: {record.Timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)}");
            builder.AppendLine();
            builder.AppendLine(NormalizeContent(record.Content));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string BuildMarkdown(
        string romId,
        DateTimeOffset generatedAt,
        IReadOnlyList<string> securityTags,
        string hash,
        string body)
    {
        var builder = new StringBuilder();
        builder.AppendLine("---");
        builder.AppendLine($"rom_id: {QuoteYaml(romId)}");
        builder.AppendLine("entity_type: 'conversation'");
        builder.AppendLine("version: '1'");
        builder.AppendLine("source_kind: 'chat_history'");
        builder.AppendLine(
            "generated_at: " +
            QuoteYaml(generatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
        builder.AppendLine("security:");
        builder.AppendLine("  tags:");

        foreach (var tag in securityTags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(tag => tag, StringComparer.Ordinal))
        {
            builder.AppendLine($"    - {QuoteYaml(tag)}");
        }

        builder.AppendLine("signature:");
        builder.AppendLine($"  hash: {QuoteYaml(hash)}");
        builder.AppendLine("---");
        builder.Append(body);
        return builder.ToString();
    }

    private static string ComputeSha256(
        string value)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string NormalizeContent(
        string content)
        => content
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Trim();

    private static string QuoteYaml(
        string value)
        => "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";

    private static string SanitizeSegment(
        string value)
        => new(
            value.Trim()
                .Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.'
                    ? c
                    : '-')
                .ToArray());
}
