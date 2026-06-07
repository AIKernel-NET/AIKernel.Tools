using System.Text.Json;

namespace AIKernel.Tools.Capability.RomStorage.ChatHistory;

public sealed class ChatHistoryProvider
{
    private readonly IReadOnlyList<ChatHistoryRecord> _records;

    public ChatHistoryProvider(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        _records = JsonSerializer.Deserialize<List<ChatHistoryRecord>>(json)
                   ?? throw new InvalidOperationException("Invalid chat history JSON.");
    }

    public IReadOnlyList<ChatHistoryRecord> GetAll() => _records;

    public IEnumerable<ChatHistoryRecord> GetByRole(string role)
        => _records.Where(r => r.Role.Equals(role, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ChatHistoryRecord> GetRecords()
    {
        return _records;
    }

    public static ChatHistoryRecord? GetLatest(IEnumerable<ChatHistoryRecord> _records)
        => _records.LastOrDefault();
}
