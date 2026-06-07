using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Tools.Capability.DynamicPipelineCompiler;

public static class DynamicPipelineCompilerCapabilityContracts
{
    public static CapabilityModuleDescriptor ToContract(
        DynamicPipelineCompilerCapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "Dynamic Pipeline Compiler",
            CapabilityModuleKind.ManagedAssembly,
            CapabilityInvocationMode.AssemblyReference,
            GetMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            "AIKernel.Tools.Capability.DynamicPipelineCompiler",
            null,
            null,
            [
                "pipeline.compile",
                "pipeline.execute",
                "pipeline.validate"
            ],
            ["dsl.read", "capability.register"],
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
