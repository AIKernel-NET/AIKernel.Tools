namespace AIKernel.Tools.Capability.DynamicPipelineCompiler;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.DynamicPipelineCompiler.DynamicPipelineCompilerCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.DynamicPipelineCompiler.DynamicPipelineCompilerCapabilityDescriptor']" />
public sealed record DynamicPipelineCompilerCapabilityDescriptor(
    string CapabilityId,
    string DslSchemaVersion,
    IReadOnlyDictionary<string, string> Metadata);
