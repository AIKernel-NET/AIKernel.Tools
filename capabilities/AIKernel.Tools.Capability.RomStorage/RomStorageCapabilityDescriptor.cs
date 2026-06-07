namespace AIKernel.Tools.Capability.RomStorage;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.RomStorageCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.RomStorage.RomStorageCapabilityDescriptor']" />
public sealed record RomStorageCapabilityDescriptor(
    string CapabilityId,
    string StorageScheme,
    IReadOnlyDictionary<string, string> Metadata);
