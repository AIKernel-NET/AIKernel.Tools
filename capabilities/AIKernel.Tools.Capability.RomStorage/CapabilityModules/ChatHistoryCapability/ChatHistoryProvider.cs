using System.Text.Json;

namespace AIKernel.Tools.Capability.RomStorage.ChatHistory;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider']" />
public sealed class ChatHistoryProvider
{
    private readonly IReadOnlyList<ChatHistoryRecord> _records;

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.#ctor']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.#ctor']" />
    public ChatHistoryProvider(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        _records = JsonSerializer.Deserialize<List<ChatHistoryRecord>>(json)
                   ?? throw new InvalidOperationException("Invalid chat history JSON.");
    }

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetAll']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetAll']" />
    public IReadOnlyList<ChatHistoryRecord> GetAll() => _records;

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetByRole']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetByRole']" />
    public IEnumerable<ChatHistoryRecord> GetByRole(string role)
        => _records.Where(r => r.Role.Equals(role, StringComparison.OrdinalIgnoreCase));

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetRecords']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetRecords']" />
    public IEnumerable<ChatHistoryRecord> GetRecords()
    {
        return _records;
    }

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetLatest']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryProvider.GetLatest']" />
    public static ChatHistoryRecord? GetLatest(IEnumerable<ChatHistoryRecord> _records)
        => _records.LastOrDefault();
}
