using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AIKernel.Tools.Capability.RomStorage.ChatHistory;

namespace AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter']" />
public static class RomExporter
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter.ToRom']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter.ToRom']" />
    public static string ToRom(IReadOnlyList<ChatHistoryRecord> records)
        => ToRom(records, "scraper", "history", generatedAtUtc: null);

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter.ToRomWithMetadata']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter.ToRomWithMetadata']" />
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
        AppendLine(builder, "# ROM:ChatHistory");
        AppendLine(builder);

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

            AppendLine(builder, $"## Turn:{i + 1}");
            AppendLine(builder, $"@role: {record.Role.Trim()}");
            AppendLine(
                builder,
                $"@time: {record.Timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)}");
            AppendLine(builder);
            AppendLine(builder, NormalizeContent(record.Content));
            AppendLine(builder);
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
        AppendLine(builder, "---");
        AppendLine(builder, $"rom_id: {QuoteYaml(romId)}");
        AppendLine(builder, "entity_type: 'conversation'");
        AppendLine(builder, "version: '1'");
        AppendLine(builder, "source_kind: 'chat_history'");
        AppendLine(
            builder,
            "generated_at: " +
            QuoteYaml(generatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
        AppendLine(builder, "security:");
        AppendLine(builder, "  tags:");

        foreach (var tag in securityTags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(tag => tag, StringComparer.Ordinal))
        {
            AppendLine(builder, $"    - {QuoteYaml(tag)}");
        }

        AppendLine(builder, "signature:");
        AppendLine(builder, $"  hash: {QuoteYaml(hash)}");
        AppendLine(builder, "---");
        builder.Append(body);
        return builder.ToString();
    }

    private static void AppendLine(
        StringBuilder builder,
        string value = "")
        => builder.Append(value).Append('\n');

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
