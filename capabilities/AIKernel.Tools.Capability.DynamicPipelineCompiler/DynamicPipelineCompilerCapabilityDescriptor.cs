namespace AIKernel.Tools.Capability.DynamicPipelineCompiler;

public sealed record DynamicPipelineCompilerCapabilityDescriptor(
    string CapabilityId,
    string DslSchemaVersion,
    IReadOnlyDictionary<string, string> Metadata);
