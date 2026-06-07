using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;
using AIKernel.Tools.Capability.ChatOpenAI;
using AIKernel.Tools.Capability.CudaCompute;
using AIKernel.Tools.Capability.DynamicPipelineCompiler;
using AIKernel.Tools.Capability.LocalLLM;
using AIKernel.Tools.Capability.RomStorage;
using AIKernel.Tools.Capability.VfsGit;

namespace AIKernel.Tools.Tests;

public sealed class CapabilityContractMapperTests
{
    [Fact]
    public void DynamicPipelineCompilerExposesDslOperations()
    {
        var metadata = Metadata();
        var contract = DynamicPipelineCompilerCapabilityContracts.ToContract(
            new DynamicPipelineCompilerCapabilityDescriptor(
                "tools.pipeline",
                "0.1",
                metadata));

        AssertContract(
            contract,
            "tools.pipeline",
            "Dynamic Pipeline Compiler",
            "AIKernel.Tools.Capability.DynamicPipelineCompiler",
            null,
            null,
            metadata,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            ["dsl.read", "capability.register"],
            "pipeline.compile",
            "pipeline.execute",
            "pipeline.validate");
    }

    [Fact]
    public void CudaComputeExposesNativeTensorOperations()
    {
        var metadata = Metadata(
            ("loader_json", "rom://capabilities/cuda/loader.json"),
            ("artifact_hash", "sha256:cuda"));
        var contract = CudaComputeCapabilityContracts.ToContract(
            new CudaComputeCapabilityDescriptor(
                "tools.cuda",
                "cuda13-win-x64",
                metadata));

        AssertContract(
            contract,
            "tools.cuda",
            "CUDA Compute",
            "libtorch_bridge",
            "rom://capabilities/cuda/loader.json",
            "sha256:cuda",
            metadata,
            CapabilityModuleKind.NativeLibrary,
            CapabilityInvocationMode.NativeAbi,
            ["native.load", "tensor.compute"],
            "tensor.matmul",
            "tensor.softmax",
            "tensor.conv2d",
            "tensor.layernorm");
    }

    [Fact]
    public void VfsGitAndRomStorageStayManagedCapabilityContracts()
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
            "AIKernel.Tools.Capability.VfsGit",
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
            "AIKernel.Tools.Capability.RomStorage",
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

    [Fact]
    public void LlmCapabilitiesExposeRemoteAndLocalContractsSeparately()
    {
        var remoteMetadata = Metadata(("endpoint", "https://example.invalid/v1"));
        var localMetadata = Metadata(
            ("runtime_uri", "ollama://localhost"),
            ("artifact_hash", "sha256:local"));
        var remote = ChatOpenAICapabilityContracts.ToContract(
            new ChatOpenAICapabilityDescriptor("tools.openai", "gpt", remoteMetadata));
        var local = LocalLLMCapabilityContracts.ToContract(
            new LocalLLMCapabilityDescriptor("tools.local", "ollama", localMetadata));

        AssertContract(
            remote,
            "tools.openai",
            "Chat OpenAI",
            "https://example.invalid/v1",
            null,
            null,
            remoteMetadata,
            CapabilityModuleKind.RemoteEndpoint,
            CapabilityInvocationMode.Remote,
            ["network.egress", "llm.remote"],
            "chat.completion",
            "embedding",
            "moderation");
        AssertContract(
            local,
            "tools.local",
            "Local LLM",
            "ollama",
            "ollama://localhost",
            "sha256:local",
            localMetadata,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            ["local.process", "llm.local"],
            "chat.local",
            "embedding.local");
    }

    private static Dictionary<string, string> Metadata(params (string Key, string Value)[] entries)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["version"] = "0.1.0.2"
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
        Assert.Equal(capabilityId, descriptor.CapabilityId);
        Assert.Equal(name, descriptor.Name);
        Assert.Equal(kind, descriptor.Kind);
        Assert.Equal(mode, descriptor.InvocationMode);
        Assert.Equal(entryPoint, descriptor.EntryPoint);
        Assert.Equal(artifactUri, descriptor.ArtifactUri);
        Assert.Equal(artifactHash, descriptor.ArtifactHash);
        Assert.Equal(operations, descriptor.ProvidedOperations);
        Assert.Equal(permissions, descriptor.RequiredPermissions);
        Assert.Equal("0.1.0.2", descriptor.Version);
        Assert.Same(metadata, descriptor.Metadata);
        Assert.Equal("0.1.0.2", descriptor.Metadata["version"]);
    }
}
