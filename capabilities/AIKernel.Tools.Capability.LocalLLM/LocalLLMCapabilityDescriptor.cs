namespace AIKernel.Tools.Capability.LocalLLM;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.LocalLLM.LocalLLMCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.LocalLLM.LocalLLMCapabilityDescriptor']" />
public sealed record LocalLLMCapabilityDescriptor(
    string CapabilityId,
    string Runtime,
    IReadOnlyDictionary<string, string> Metadata);
