namespace AIKernel.Tools.Capability.RomStorage.ChatHistory.Models;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.ChatHistory.Models.ChatHistory']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.ChatHistory.Models.ChatHistory']" />
public sealed class ChatHistory(IReadOnlyList<ChatMessage> messages)
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.Models.ChatHistory.Messages']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='P:AIKernel.Tools.Capability.RomStorage.ChatHistory.Models.ChatHistory.Messages']" />
    public IReadOnlyList<ChatMessage> Messages { get; } = messages;
}
