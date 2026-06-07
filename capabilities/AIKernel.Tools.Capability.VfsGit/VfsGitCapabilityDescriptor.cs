namespace AIKernel.Tools.Capability.VfsGit;

public sealed record VfsGitCapabilityDescriptor(
    string CapabilityId,
    string RepositoryMode,
    IReadOnlyDictionary<string, string> Metadata);
