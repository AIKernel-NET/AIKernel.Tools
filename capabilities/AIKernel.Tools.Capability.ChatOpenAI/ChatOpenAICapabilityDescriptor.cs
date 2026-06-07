namespace AIKernel.Tools.Capability.ChatOpenAI;

public sealed record ChatOpenAICapabilityDescriptor(
    string CapabilityId,
    string Model,
    IReadOnlyDictionary<string, string> Metadata);
