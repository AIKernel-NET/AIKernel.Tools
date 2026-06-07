using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Tools.Capability.LocalLLM;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.LocalLLM.LocalLLMCapabilityContracts']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.LocalLLM.LocalLLMCapabilityContracts']" />
public static class LocalLLMCapabilityContracts
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.LocalLLM.LocalLLMCapabilityContracts.ToContract']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.LocalLLM.LocalLLMCapabilityContracts.ToContract']" />
    public static CapabilityModuleDescriptor ToContract(
        LocalLLMCapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "Local LLM",
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            GetRequiredMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            descriptor.Runtime,
            GetMetadataValue(descriptor.Metadata, "runtime_uri", null),
            GetMetadataValue(descriptor.Metadata, "artifact_hash", null),
            [
                "chat.local",
                "embedding.local"
            ],
            ["local.process", "llm.local"],
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
