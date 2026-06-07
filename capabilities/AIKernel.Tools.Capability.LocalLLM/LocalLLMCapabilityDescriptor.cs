namespace AIKernel.Tools.Capability.LocalLLM;

public sealed record LocalLLMCapabilityDescriptor(
    string CapabilityId,
    string Runtime,
    IReadOnlyDictionary<string, string> Metadata);
