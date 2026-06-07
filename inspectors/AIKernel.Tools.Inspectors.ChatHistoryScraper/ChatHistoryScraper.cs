using AIKernel.Tools.Capability.RomStorage.ChatHistory;
using AIKernel.Tools.Capability.RomStorage.ChatHistory.Models;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIKernel.Tools.Inspectors.ChatHistoryScraper;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper']" />
public static partial class ChatHistoryScraper
{
    private static readonly HttpClient Http = new();

    // ------------------------------
    // 1. Export entry point
    // ------------------------------
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper.ExportAsync']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper.ExportAsync']" />
    public static async Task<ChatHistory> ExportAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required.", nameof(url));

        var html = await Http.GetStringAsync(url);
        var stream = ExtractStreamPayload(html);

        if (!stream.Contains("conversation-turn"))
            throw new InvalidOperationException("Conversation data not found in shared page.");

        var firstJson = ExtractFirstJsonTable(stream);
        var table = JsonSerializer.Deserialize<JsonElement[]>(firstJson)
            ?? throw new InvalidOperationException("Invalid JSON table.");

        var inflated = Inflate(table, 0, []);
        var messages = ExtractMessages(inflated);

        return new ChatHistory(messages);
    }
    // ------------------------------
    // Convert ChatHistory -> ChatHistoryRecord[]
    // ------------------------------
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper.ToRecords']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper.ToRecords']" />
    public static IReadOnlyList<ChatHistoryRecord> ToRecords(ChatHistory history)
    {
        if (history is null) return [];

        return [.. history.Messages
            .Select(m =>
            {
                var ts = ParseChatTimestamp(m.Timestamp);

                return new ChatHistoryRecord
                {
                    Role = m.Role ?? "user",
                    Content = m.Content ?? string.Empty,
                    Timestamp = ts
                };
            })];
    }


    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper.ToMarkdown']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper.ToMarkdown']" />
    public static string ToMarkdown(ChatHistory history, string sourceUrl)
    {
        var lines = new List<string>
        {
            "# ChatGPT Share Export",
            "",
            $"Source: {sourceUrl}",
            ""
        };

        for (var i = 0; i < history.Messages.Count; i++)
        {
            var message = history.Messages[i];
            lines.Add($"## {i + 1}. {NormalizeRole(message.Role)}");
            lines.Add("");
            lines.Add(message.Content);
            lines.Add("");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string NormalizeRole(string role)
        => role.ToLowerInvariant() switch
        {
            "system" => "System",
            "user" => "User",
            "assistant" => "Assistant",
            "tool" => "Tool",
            _ => role
        };

    // ------------------------------
    // 2. Extract stream payload
    // ------------------------------
    private static string ExtractStreamPayload(string html)
    {
        var scripts = ScriptTagRegex.Matches(html)
            .Select(m => m.Groups[1].Value)
            .Where(s => s.Contains("streamController.enqueue"));

        var chunks = new List<string>();

        foreach (var script in scripts)
        {
            foreach (Match m in StreamEnqueueRegex.Matches(script))
            {
                var json = JsonSerializer.Deserialize<string>(m.Groups[1].Value);
                if (json != null)
                    chunks.Add(json);
            }
        }

        return string.Concat(chunks);
    }

    // ------------------------------
    // 3. Inflate JSON (Node.js 移植)
    // ------------------------------
    private static object? Inflate(JsonElement[] table, int index, Dictionary<int, object?> cache)
    {
        if (cache.TryGetValue(index, out var cached))
            return cached;

        var elem = table[index];
        object? result = elem.ValueKind switch
        {
            JsonValueKind.Number => InflateNumber(table, elem, cache),
            JsonValueKind.String => elem.GetString(),
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => InflateArray(table, elem, index, cache),
            JsonValueKind.Object => InflateObject(table, elem, index, cache),
            _ => null
        };

        return result;
    }

    private static List<object?> InflateArray(JsonElement[] table, JsonElement elem, int index, Dictionary<int, object?> cache)
    {
        var list = new List<object?>();
        cache[index] = list;

        foreach (var item in elem.EnumerateArray())
        {
            list.Add(InflateValue(table, item, cache));
        }

        return list;
    }

    private static Dictionary<string, object?> InflateObject(JsonElement[] table, JsonElement elem, int index, Dictionary<int, object?> cache)
    {
        var dict = new Dictionary<string, object?>();
        cache[index] = dict;

        foreach (var prop in elem.EnumerateObject())
        {
            var key = prop.Name;
            if (key.Length > 0 && key[0] == '_' && int.TryParse(key[1..], out var refKey))
                key = InflateRef(table, refKey, cache)?.ToString() ?? key;

            dict[key] = InflateValue(table, prop.Value, cache);
        }

        return dict;
    }

    private static string ExtractFirstJsonTable(string stream)
    {
        var trimmed = stream.Trim();
        var marker = ExtractFirstTableRegex().Match(trimmed);
        return marker.Success
            ? trimmed[..marker.Index]
            : trimmed;
    }

    private static object? InflateValue(JsonElement[] table, JsonElement elem, Dictionary<int, object?> cache)
        => elem.ValueKind switch
        {
            JsonValueKind.Number => InflateNumber(table, elem, cache),
            JsonValueKind.String => elem.GetString(),
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => InflateInlineArray(table, elem, cache),
            JsonValueKind.Object => InflateInlineObject(table, elem, cache),
            _ => null
        };

    private static object? InflateNumber(JsonElement[] table, JsonElement elem, Dictionary<int, object?> cache)
    {
        if (elem.TryGetInt32(out var index)
            && (index is -1 or -2 or -3 or -4 or -5 || index >= 0 && index < table.Length))
        {
            return InflateRef(table, index, cache);
        }

        return elem.TryGetInt64(out var longValue)
            ? longValue
            : elem.GetDouble();
    }

    private static List<object?> InflateInlineArray(JsonElement[] table, JsonElement elem, Dictionary<int, object?> cache)
    {
        var list = new List<object?>();

        foreach (var item in elem.EnumerateArray())
            list.Add(InflateValue(table, item, cache));

        return list;
    }

    private static Dictionary<string, object?> InflateInlineObject(JsonElement[] table, JsonElement elem, Dictionary<int, object?> cache)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var prop in elem.EnumerateObject())
        {
            var key = prop.Name;
            if (key.Length > 0 && key[0] == '_' && int.TryParse(key[1..], out var refKey))
                key = InflateRef(table, refKey, cache)?.ToString() ?? key;

            dict[key] = InflateValue(table, prop.Value, cache);
        }

        return dict;
    }

    private static object? InflateRef(JsonElement[] table, int index, Dictionary<int, object?> cache)
        => index switch
        {
            -1 or -2 or -5 => null,
            -3 => double.NaN,
            -4 => double.PositiveInfinity,
            _ => Inflate(table, index, cache)
        };

    // ------------------------------
    // 4. Extract messages
    // ------------------------------
    private static List<ChatMessage> ExtractMessages(object? root)
    {
        var messages = new List<ChatMessage>();
        var visited = new HashSet<object>();
        var messageKeys = new HashSet<string>(StringComparer.Ordinal);
        Walk(root, messages, visited, messageKeys);
        return messages;
    }

    private static void Walk(
        object? value,
        List<ChatMessage> messages,
        HashSet<object> visited,
        HashSet<string> messageKeys)
    {
        if (value is null || visited.Contains(value))
            return;

        visited.Add(value);

        switch (value)
        {
            case Dictionary<string, object?> dict:
                ExtractMessageIfPresent(dict, messages, messageKeys);
                foreach (var v in dict.Values)
                    Walk(v, messages, visited, messageKeys);
                break;

            case List<object?> list:
                foreach (var item in list)
                    Walk(item, messages, visited, messageKeys);
                break;
        }
    }

    private static void ExtractMessageIfPresent(
        Dictionary<string, object?> dict,
        List<ChatMessage> messages,
        HashSet<string> messageKeys)
    {
        var role = TryGetNestedString(dict, "author", "role")
            ?? dict.GetValueOrDefault("author_role")
            ?? dict.GetValueOrDefault("role");
        if (role is not string r)
            return;

        var text = ExtractText(dict.GetValueOrDefault("content"));
        if (string.IsNullOrWhiteSpace(text))
            return;

        text = text.Trim();
        if (!ShouldExportMessage(r, dict.GetValueOrDefault("recipient") as string, text))
            return;

        var key = dict.GetValueOrDefault("id") is string id && !string.IsNullOrWhiteSpace(id)
            ? id
            : $"{r}\u001f{text}";

        if (!messageKeys.Add(key))
            return;

        messages.Add(new ChatMessage(
            Role: r,
            Content: text,
            Timestamp: ExtractTimestamp(dict)
        ));
    }

    private static string ExtractTimestamp(Dictionary<string, object?> dict)
    {
        var timestamp =
            dict.GetValueOrDefault("create_time")
            ?? dict.GetValueOrDefault("createTime")
            ?? dict.GetValueOrDefault("update_time")
            ?? dict.GetValueOrDefault("updateTime");

        return timestamp switch
        {
            null => "",
            DateTimeOffset value => value.ToString("O", CultureInfo.InvariantCulture),
            DateTime value => new DateTimeOffset(value.ToUniversalTime(), TimeSpan.Zero).ToString("O", CultureInfo.InvariantCulture),
            long value => UnixSecondsToDateTimeOffset(value).ToString("O", CultureInfo.InvariantCulture),
            int value => UnixSecondsToDateTimeOffset(value).ToString("O", CultureInfo.InvariantCulture),
            double value => UnixSecondsToDateTimeOffset(value).ToString("O", CultureInfo.InvariantCulture),
            float value => UnixSecondsToDateTimeOffset(value).ToString("O", CultureInfo.InvariantCulture),
            decimal value => UnixSecondsToDateTimeOffset((double)value).ToString("O", CultureInfo.InvariantCulture),
            string value => NormalizeTimestampString(value),
            _ => timestamp.ToString() ?? ""
        };
    }

    private static DateTimeOffset ParseChatTimestamp(string timestamp)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
            throw new InvalidOperationException("Chat message timestamp was not found in the shared conversation data.");

        if (DateTimeOffset.TryParse(
                timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return parsed;
        }

        if (long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixLong))
            return UnixSecondsToDateTimeOffset(unixLong);

        if (double.TryParse(timestamp, NumberStyles.Float, CultureInfo.InvariantCulture, out var unixDouble))
            return UnixSecondsToDateTimeOffset(unixDouble);

        throw new FormatException($"Chat message timestamp is not a supported format: '{timestamp}'.");
    }

    private static string NormalizeTimestampString(string timestamp)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
            return "";

        return ParseChatTimestamp(timestamp).ToString("O", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset UnixSecondsToDateTimeOffset(long unixValue)
    {
        return Math.Abs(unixValue) > 100_000_000_000
            ? DateTimeOffset.FromUnixTimeMilliseconds(unixValue).ToUniversalTime()
            : DateTimeOffset.FromUnixTimeSeconds(unixValue).ToUniversalTime();
    }

    private static DateTimeOffset UnixSecondsToDateTimeOffset(double unixSeconds)
    {
        if (double.IsNaN(unixSeconds) || double.IsInfinity(unixSeconds))
            throw new FormatException($"Chat message timestamp is not finite: '{unixSeconds}'.");

        if (Math.Abs(unixSeconds) > 100_000_000_000)
        {
            var milliseconds = (long)Math.Round(unixSeconds, MidpointRounding.AwayFromZero);
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToUniversalTime();
        }

        var wholeSeconds = Math.Truncate(unixSeconds);
        var fractionalSeconds = unixSeconds - wholeSeconds;

        return DateTimeOffset
            .FromUnixTimeSeconds((long)wholeSeconds)
            .AddTicks((long)Math.Round(fractionalSeconds * TimeSpan.TicksPerSecond, MidpointRounding.AwayFromZero))
            .ToUniversalTime();
    }

    private static bool ShouldExportMessage(string role, string? recipient, string text)
    {
        if (string.Equals(role, "tool", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(recipient)
            && !string.Equals(recipient, "all", StringComparison.Ordinal))
        {
            return false;
        }

        if (text.StartsWith("思考時間:", StringComparison.Ordinal))
            return false;

        return !ToolCallTextRegex().IsMatch(text);
    }

    private static string? TryGetNestedString(Dictionary<string, object?> dict, string objectKey, string valueKey)
    {
        return dict.GetValueOrDefault(objectKey) is Dictionary<string, object?> nested
            && nested.GetValueOrDefault(valueKey) is string value
            ? value
            : null;
    }

    // ------------------------------
    // 5. Extract text
    // ------------------------------
    private static string ExtractText(object? content) =>
        content switch
        {
            null => "",
            string s => s,
            List<object?> list => string.Join("\n\n", list.Select(ExtractText)),
            Dictionary<string, object?> dict => ExtractTextFromDict(dict),
            _ => ""
        };

    private static string ExtractTextFromDict(Dictionary<string, object?> dict)
    {
        if (dict.TryGetValue("text", out var t) && t is string ts)
            return ts;

        if (dict.TryGetValue("content", out var c) && c is string cs)
            return cs;

        if (dict.TryGetValue("parts", out var p) && p is List<object?> parts)
            return string.Join("\n\n", parts.Select(ExtractText));

        return "";
    }

    // ------------------------------
    // Regex (static readonly)
    // ------------------------------
    private static readonly Regex ScriptTagRegex =
        HtmlScriptTag();

    private static readonly Regex StreamEnqueueRegex =
        StreamControllerEnqueueRegex();

    [GeneratedRegex("<script[^>]*>([\\s\\S]*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled, "ja-JP")]
    private static partial Regex HtmlScriptTag();
    [GeneratedRegex("streamController\\.enqueue\\((\"(?:\\\\.|[^\"\\\\])*\")\\)", RegexOptions.Compiled)]
    private static partial Regex StreamControllerEnqueueRegex();

    [GeneratedRegex("^(search|open|click|find|screenshot|finance|weather|sports|time)\\(", RegexOptions.Compiled)]
    private static partial Regex ToolCallTextRegex();
    [GeneratedRegex("\n[A-Z]?\\d+:")]
    private static partial Regex ExtractFirstTableRegex();
}
