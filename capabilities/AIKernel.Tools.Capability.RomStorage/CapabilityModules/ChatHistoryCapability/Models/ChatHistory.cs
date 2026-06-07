namespace AIKernel.Tools.Capability.RomStorage.ChatHistory.Models;

public sealed class ChatHistory(IReadOnlyList<ChatMessage> messages)
{
    public IReadOnlyList<ChatMessage> Messages { get; } = messages;
}
