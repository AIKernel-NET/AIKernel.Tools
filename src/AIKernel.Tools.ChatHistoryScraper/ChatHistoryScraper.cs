using AIKernel.Tools.ChatHistoryScraper.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIKernel.Tools.ChatHistoryScraper;

public static partial class ChatHistoryScraper
{
    private static readonly HttpClient Http = new();

    // ------------------------------
    // 1. Export entry point
    // ------------------------------
    public static async Task<ChatHistory> ExportAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required.", nameof(url));

        var html = await Http.GetStringAsync(url);
        var stream = ExtractStreamPayload(html);

        if (!stream.Contains("conversation-turn"))
            throw new InvalidOperationException("Conversation data not found in shared page.");

        var firstJson = stream.Trim().Split('\n')[0];
        var table = JsonSerializer.Deserialize<JsonElement[]>(firstJson)
            ?? throw new InvalidOperationException("Invalid JSON table.");

        var inflated = Inflate(table, 0, []);
        var messages = ExtractMessages(inflated);

        return new ChatHistory(messages);
    }

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
            JsonValueKind.Number => InflateRef(table, elem.GetInt32(), cache),
            JsonValueKind.String => elem.GetString(),
            JsonValueKind.Null => null,
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
            list.Add(item.ValueKind == JsonValueKind.Number
                ? InflateRef(table, item.GetInt32(), cache)
                : InflateValue(table, item, cache));
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

    private static object? InflateValue(JsonElement[] table, JsonElement elem, Dictionary<int, object?> cache)
        => elem.ValueKind switch
        {
            JsonValueKind.Number => InflateRef(table, elem.GetInt32(), cache),
            JsonValueKind.String => elem.GetString(),
            JsonValueKind.Null => null,
            _ => elem.ToString()
        };

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
        Walk(root, messages, visited);
        return messages;
    }

    private static void Walk(object? value, List<ChatMessage> messages, HashSet<object> visited)
    {
        if (value is null || visited.Contains(value))
            return;

        visited.Add(value);

        switch (value)
        {
            case Dictionary<string, object?> dict:
                ExtractMessageIfPresent(dict, messages);
                foreach (var v in dict.Values)
                    Walk(v, messages, visited);
                break;

            case List<object?> list:
                foreach (var item in list)
                    Walk(item, messages, visited);
                break;
        }
    }

    private static void ExtractMessageIfPresent(Dictionary<string, object?> dict, List<ChatMessage> messages)
    {
        var role = dict.GetValueOrDefault("author_role") ?? dict.GetValueOrDefault("role");
        if (role is not string r)
            return;

        var text = ExtractText(dict.GetValueOrDefault("content"));
        if (string.IsNullOrWhiteSpace(text))
            return;

        messages.Add(new ChatMessage(
            Role: r,
            Text: text.Trim(),
            Timestamp: dict.GetValueOrDefault("create_time")?.ToString()
        ));
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
}
