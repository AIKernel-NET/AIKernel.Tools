namespace AIKernel.Tools.CapabilityModules.ChatHistoryCapability;

public sealed class ChatHistoryRecord
{
    public required string Role { get; init; }   // "user" / "assistant"
    public required string Content { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
