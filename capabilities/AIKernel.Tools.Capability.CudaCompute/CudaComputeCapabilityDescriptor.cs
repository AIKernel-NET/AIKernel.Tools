namespace AIKernel.Tools.Capability.CudaCompute;

public sealed record CudaComputeCapabilityDescriptor(
    string CapabilityId,
    string DeviceProfile,
    IReadOnlyDictionary<string, string> Metadata);
