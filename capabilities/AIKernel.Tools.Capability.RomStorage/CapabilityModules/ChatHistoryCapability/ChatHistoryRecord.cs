namespace AIKernel.Tools.Capability.RomStorage.ChatHistory;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord']" />
public sealed class ChatHistoryRecord
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord.Role']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord.Role']" />
    public required string Role { get; init; }
    /// <include file="docs.en.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord.Content']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord.Content']" />
    public required string Content { get; init; }
    /// <include file="docs.en.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord.Timestamp']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.ChatHistoryRecord.Timestamp']" />
    public DateTimeOffset Timestamp { get; init; }
}
