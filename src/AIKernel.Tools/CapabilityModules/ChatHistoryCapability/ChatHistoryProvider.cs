using System.Text.Json;

namespace AIKernel.Tools.CapabilityModules.ChatHistoryCapability;

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

    public ChatHistoryRecord? GetLatest()
        => _records.LastOrDefault();
}
