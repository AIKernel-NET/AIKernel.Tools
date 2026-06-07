namespace AIKernel.Tools.Capability.CudaCompute;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.CudaCompute.CudaComputeCapabilityDescriptor']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Capability.CudaCompute.CudaComputeCapabilityDescriptor']" />
public sealed record CudaComputeCapabilityDescriptor(
    string CapabilityId,
    string DeviceProfile,
    IReadOnlyDictionary<string, string> Metadata);
