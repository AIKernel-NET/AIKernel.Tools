namespace AIKernel.Tools.Capability.RomStorage;

public sealed record RomStorageCapabilityDescriptor(
    string CapabilityId,
    string StorageScheme,
    IReadOnlyDictionary<string, string> Metadata);
