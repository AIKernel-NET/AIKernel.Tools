using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Tools.Capability.VfsGit;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.VfsGit.VfsGitCapabilityContracts']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.VfsGit.VfsGitCapabilityContracts']" />
public static class VfsGitCapabilityContracts
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.VfsGit.VfsGitCapabilityContracts.ToContract']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Capability.VfsGit.VfsGitCapabilityContracts.ToContract']" />
    public static CapabilityModuleDescriptor ToContract(
        VfsGitCapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "VFS Git",
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            GetMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            "AIKernel.Tools.Capability.VfsGit",
            null,
            null,
            [
                "vfs.git.read",
                "vfs.git.list",
                "vfs.git.checkout"
            ],
            ["vfs.read", "git.read"],
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
