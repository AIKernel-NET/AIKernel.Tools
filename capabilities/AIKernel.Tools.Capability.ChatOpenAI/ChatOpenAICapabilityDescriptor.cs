namespace AIKernel.Tools.Capability.ChatOpenAI;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.ChatOpenAI.ChatOpenAICapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.ChatOpenAI.ChatOpenAICapabilityDescriptor']" />
public sealed record ChatOpenAICapabilityDescriptor(
    string CapabilityId,
    string Model,
    IReadOnlyDictionary<string, string> Metadata);
