using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Tools.Capability.CudaCompute;

public static class CudaComputeCapabilityContracts
{
    public static CapabilityModuleDescriptor ToContract(
        CudaComputeCapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "CUDA Compute",
            CapabilityModuleKind.NativeLibrary,
            CapabilityInvocationMode.NativeAbi,
            GetRequiredMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            "libtorch_bridge",
            GetMetadataValue(descriptor.Metadata, "loader_json", null),
            GetMetadataValue(descriptor.Metadata, "artifact_hash", null),
            [
                "tensor.matmul",
                "tensor.softmax",
                "tensor.conv2d",
                "tensor.layernorm"
            ],
            ["native.load", "tensor.compute"],
            descriptor.Metadata);
    }

    private static string? GetMetadataValue(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string? fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    private static string GetRequiredMetadataValue(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
}
