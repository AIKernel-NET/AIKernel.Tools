using AIKernel.Dtos.Capabilities;
using CoreRomStorageCapabilityContracts = AIKernel.Core.Storage.RomStorageCapabilityContracts;
using CoreRomStorageCapabilityDescriptor = AIKernel.Core.Storage.RomStorageCapabilityDescriptor;

namespace AIKernel.Tools.Capability.RomStorage;

/// <summary>
/// [EN] Python bridge that delegates to RomStorageCapabilityContracts.
/// [JA] RomStorageCapabilityContracts に委譲する Python bridge です。
/// </summary>
public static class RomStoragePythonBridge
{
    /// <summary>
    /// [EN] Creates the public capability contract for Python callers.
    /// [JA] Python caller 向けに公開 capability contract を作成します。
    /// </summary>
    public static CapabilityModuleDescriptor ToContract(
        string capabilityId,
        string storageScheme,
        IReadOnlyDictionary<string, string> metadata)
        => CoreRomStorageCapabilityContracts.ToContract(
            new CoreRomStorageCapabilityDescriptor(
                capabilityId,
                storageScheme,
                metadata));
}
