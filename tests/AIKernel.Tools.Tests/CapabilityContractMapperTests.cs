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
        var contract = DynamicPipelineCompilerCapabilityContracts.ToContract(
            new DynamicPipelineCompilerCapabilityDescriptor(
                "tools.pipeline",
                "0.1",
                Metadata()));

        AssertContract(
            contract,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            "pipeline.compile",
            "pipeline.execute",
            "pipeline.validate");
    }

    [Fact]
    public void CudaComputeExposesNativeTensorOperations()
    {
        var contract = CudaComputeCapabilityContracts.ToContract(
            new CudaComputeCapabilityDescriptor(
                "tools.cuda",
                "cuda13-win-x64",
                Metadata()));

        Assert.Equal(CapabilityModuleKind.NativeLibrary, contract.Kind);
        Assert.Equal(CapabilityInvocationMode.NativeAbi, contract.InvocationMode);
        Assert.Contains("tensor.matmul", contract.ProvidedOperations);
        Assert.Contains("tensor.softmax", contract.ProvidedOperations);
    }

    [Fact]
    public void VfsGitAndRomStorageStayManagedCapabilityContracts()
    {
        var git = VfsGitCapabilityContracts.ToContract(
            new VfsGitCapabilityDescriptor("tools.vfs.git", "readonly", Metadata()));
        var rom = RomStorageCapabilityContracts.ToContract(
            new RomStorageCapabilityDescriptor("tools.rom", "rom", Metadata()));

        AssertContract(
            git,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            "vfs.git.read",
            "vfs.git.list",
            "vfs.git.checkout");
        AssertContract(
            rom,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            "rom.save",
            "rom.load",
            "rom.list");
    }

    [Fact]
    public void LlmCapabilitiesExposeRemoteAndLocalContractsSeparately()
    {
        var remote = ChatOpenAICapabilityContracts.ToContract(
            new ChatOpenAICapabilityDescriptor("tools.openai", "gpt", Metadata()));
        var local = LocalLLMCapabilityContracts.ToContract(
            new LocalLLMCapabilityDescriptor("tools.local", "ollama", Metadata()));

        AssertContract(
            remote,
            CapabilityModuleKind.RemoteEndpoint,
            CapabilityInvocationMode.Remote,
            "chat.completion",
            "embedding",
            "moderation");
        AssertContract(
            local,
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            "chat.local",
            "embedding.local");
    }

    private static Dictionary<string, string> Metadata()
        => new(StringComparer.Ordinal)
        {
            ["version"] = "0.1.0.2"
        };

    private static void AssertContract(
        CapabilityModuleDescriptor descriptor,
        CapabilityModuleKind kind,
        CapabilityInvocationMode mode,
        params string[] operations)
    {
        Assert.Equal(kind, descriptor.Kind);
        Assert.Equal(mode, descriptor.InvocationMode);
        Assert.All(operations, operation => Assert.Contains(operation, descriptor.ProvidedOperations));
        Assert.Equal("0.1.0.2", descriptor.Version);
    }
}
