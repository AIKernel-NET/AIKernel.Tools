namespace AIKernel.Tools.CapabilityModules.ChatHistoryCapability.Models;

public sealed class ChatHistory(IReadOnlyList<ChatMessage> messages)
{
    public IReadOnlyList<ChatMessage> Messages { get; } = messages;
}
