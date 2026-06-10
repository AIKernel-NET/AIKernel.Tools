using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;
using AIKernel.Core.Storage;
using AIKernel.Core.Vfs.VfsGit;
using System.Text.RegularExpressions;

namespace AIKernel.Tools.Tests;

public sealed class CapabilityContractMapperTests
{
    private static readonly Regex CapabilityIdPattern =
        new(@"^tools(\.[a-z][a-z0-9]*)+$", RegexOptions.CultureInvariant);

    private static readonly Regex UriSchemePattern =
        new(@"^[a-z][a-z0-9+.-]*://", RegexOptions.CultureInvariant);

    [Fact]
    public void MetadataKeepsDeterministicInsertionOrderForContractExport()
    {
        var metadata = Metadata(
            ("storage_uri", "rom://tools/storage"),
            ("artifact_hash", "sha256:rom"));
        var contract = RomStorageCapabilityContracts.ToContract(
            new RomStorageCapabilityDescriptor(
                "tools.rom",
                "rom",
                metadata));

        Assert.Equal(
            ["version", "storage_uri", "artifact_hash"],
            contract.Metadata.Keys.ToArray());
    }

    [Fact]
    public void VfsGitAndRomStorageStayCoreOwnedCapabilityContracts()
    {
        var gitMetadata = Metadata();
        var romMetadata = Metadata();
        var git = VfsGitCapabilityContracts.ToContract(
            new VfsGitCapabilityDescriptor("tools.vfs.git", "readonly", gitMetadata));
        var rom = RomStorageCapabilityContracts.ToContract(
            new RomStorageCapabilityDescriptor("tools.rom", "rom", romMetadata));

        AssertContract(
            git,
            "tools.vfs.git",
            "VFS Git",
            "AIKernel.Core.Vfs.VfsGit",
            null,
            null,
            gitMetadata,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            ["vfs.read", "git.read"],
            "vfs.git.read",
            "vfs.git.list",
            "vfs.git.checkout");
        AssertContract(
            rom,
            "tools.rom",
            "ROM Storage",
            "AIKernel.Core.Storage",
            null,
            null,
            romMetadata,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            ["rom.read", "rom.write"],
            "rom.save",
            "rom.load",
            "rom.list");
    }

    private static Dictionary<string, string> Metadata(params (string Key, string Value)[] entries)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["version"] = "0.1.0"
        };

        foreach (var (key, value) in entries)
        {
            metadata[key] = value;
        }

        return metadata;
    }

    private static void AssertContract(
        CapabilityModuleDescriptor descriptor,
        string capabilityId,
        string name,
        string? entryPoint,
        string? artifactUri,
        string? artifactHash,
        IReadOnlyDictionary<string, string> metadata,
        CapabilityModuleKind kind,
        CapabilityInvocationMode mode,
        IReadOnlyList<string> permissions,
        params string[] operations)
    {
        // Contract-facing capability ids are stable dot-separated slugs.
        // Human-readable names are also contract text: changing them is a user-visible
        // descriptor change and should be intentional.
        Assert.Equal(capabilityId, descriptor.CapabilityId);
        Assert.Matches(CapabilityIdPattern, descriptor.CapabilityId);
        Assert.Equal(name, descriptor.Name);
        Assert.Equal(kind, descriptor.Kind);
        Assert.Equal(mode, descriptor.InvocationMode);
        Assert.Equal(entryPoint, descriptor.EntryPoint);
        Assert.Equal(artifactUri, descriptor.ArtifactUri);
        Assert.Equal(artifactHash, descriptor.ArtifactHash);

        if (artifactUri is not null)
        {
            Assert.Matches(UriSchemePattern, artifactUri);
        }

        Assert.Equal(operations, descriptor.ProvidedOperations);

        // Operation and permission order is part of the descriptor contract. It feeds
        // deterministic ROM export, hashing, PDP display, and human review diffs.
        Assert.Equal(permissions, descriptor.RequiredPermissions);
        Assert.All(descriptor.RequiredPermissions, permission => Assert.Contains('.', permission));
        Assert.Equal("0.1.0", descriptor.Version);
        Assert.Same(metadata, descriptor.Metadata);
        Assert.Equal("0.1.0", descriptor.Metadata["version"]);
    }
}
