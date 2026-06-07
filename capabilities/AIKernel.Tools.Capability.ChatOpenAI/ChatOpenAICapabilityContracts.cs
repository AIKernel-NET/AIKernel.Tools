using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Tools.Capability.ChatOpenAI;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.ChatOpenAI.ChatOpenAICapabilityContracts']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.ChatOpenAI.ChatOpenAICapabilityContracts']" />
public static class ChatOpenAICapabilityContracts
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.ChatOpenAI.ChatOpenAICapabilityContracts.ToContract']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.ChatOpenAI.ChatOpenAICapabilityContracts.ToContract']" />
    public static CapabilityModuleDescriptor ToContract(
        ChatOpenAICapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "Chat OpenAI",
            CapabilityModuleKind.RemoteEndpoint,
            CapabilityInvocationMode.Remote,
            GetRequiredMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            GetMetadataValue(descriptor.Metadata, "endpoint", null),
            null,
            null,
            [
                "chat.completion",
                "embedding",
                "moderation"
            ],
            ["network.egress", "llm.remote"],
            descriptor.Metadata);
    }

    private static string? GetMetadataValue(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string? fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private static string GetRequiredMetadataValue(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
}
