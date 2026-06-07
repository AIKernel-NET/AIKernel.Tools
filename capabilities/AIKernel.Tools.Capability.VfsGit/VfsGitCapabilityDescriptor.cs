namespace AIKernel.Tools.Capability.VfsGit;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.VfsGit.VfsGitCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.VfsGit.VfsGitCapabilityDescriptor']" />
public sealed record VfsGitCapabilityDescriptor(
    string CapabilityId,
    string RepositoryMode,
    IReadOnlyDictionary<string, string> Metadata);
