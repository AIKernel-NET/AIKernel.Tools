using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Tools.Capability.RomStorage;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.RomStorageCapabilityContracts']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.RomStorageCapabilityContracts']" />
public static class RomStorageCapabilityContracts
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.RomStorageCapabilityContracts.ToContract']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.RomStorage.RomStorageCapabilityContracts.ToContract']" />
    public static CapabilityModuleDescriptor ToContract(
        RomStorageCapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "ROM Storage",
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            GetMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            "AIKernel.Tools.Capability.RomStorage",
            null,
            null,
            [
                "rom.save",
                "rom.load",
                "rom.list"
            ],
            ["rom.read", "rom.write"],
            descriptor.Metadata);
    }

    private static string GetMetadataValue(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
}
