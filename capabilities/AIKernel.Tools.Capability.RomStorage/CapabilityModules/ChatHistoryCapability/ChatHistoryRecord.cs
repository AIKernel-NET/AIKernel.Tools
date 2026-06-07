namespace AIKernel.Tools.Capability.RomStorage.ChatHistory;

public sealed class ChatHistoryRecord
{
    public required string Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
