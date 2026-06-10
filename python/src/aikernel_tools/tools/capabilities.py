from __future__ import annotations

from dataclasses import dataclass, field
from typing import Mapping

from .managed import call_static, create_managed, to_dictionary


@dataclass(frozen=True)
class CapabilityContract:
    """[EN]
    Python view of AIKernel.Dtos.Capabilities.CapabilityModuleDescriptor.

    [JA]
    AIKernel.Dtos.Capabilities.CapabilityModuleDescriptor の Python view です。
    """

    _managed: object

    @property
    def managed(self):
        """[EN] Return the underlying C# descriptor.

        [JA] 背後の C# descriptor を返します。
        """
        return self._managed

    def to_managed(self):
        """[EN] Return the underlying C# descriptor.

        [JA] 背後の C# descriptor を返します。
        """
        return self._managed

    @property
    def capability_id(self) -> str:
        return str(self.managed.CapabilityId)

    @property
    def name(self) -> str:
        return str(self.managed.Name)

    @property
    def kind(self) -> str:
        return str(self.managed.Kind)

    @property
    def invocation_mode(self) -> str:
        return str(self.managed.InvocationMode)

    @property
    def version(self) -> str:
        return str(self.managed.Version)

    @property
    def entry_point(self) -> str | None:
        value = self.managed.EntryPoint
        return None if value is None else str(value)

    @property
    def artifact_uri(self) -> str | None:
        value = self.managed.ArtifactUri
        return None if value is None else str(value)

    @property
    def artifact_hash(self) -> str | None:
        value = self.managed.ArtifactHash
        return None if value is None else str(value)

    @property
    def provided_operations(self) -> tuple[str, ...]:
        return tuple(str(value) for value in self.managed.ProvidedOperations)

    @property
    def required_permissions(self) -> tuple[str, ...]:
        return tuple(str(value) for value in self.managed.RequiredPermissions)

    @property
    def metadata(self) -> dict[str, str]:
        return {str(key): str(self.managed.Metadata[key]) for key in self.managed.Metadata.Keys}


@dataclass(frozen=True)
class RomStorageCapability:
    """[EN] Wrapper for ROM storage capability contracts.

    [JA] ROM storage capability contract の wrapper です。
    """

    capability_id: str
    storage_scheme: str = "rom"
    metadata: Mapping[str, str] = field(default_factory=dict)

    def _metadata(self):
        return _metadata(self.metadata)

    def to_contract(self) -> CapabilityContract:
        return CapabilityContract(
            call_static(
                "AIKernel.Core.Storage.RomStorageCapabilityContracts",
                "AIKernel.Core",
                "ToContract",
                create_managed(
                    "AIKernel.Core.Storage.RomStorageCapabilityDescriptor",
                    "AIKernel.Core",
                    self.capability_id,
                    self.storage_scheme,
                    self._metadata(),
                ),
            )
        )


@dataclass(frozen=True)
class VfsGitCapability:
    """[EN] Wrapper for VFS Git capability contracts.

    [JA] VFS Git capability contract の wrapper です。
    """

    capability_id: str
    repository_mode: str = "readonly"
    metadata: Mapping[str, str] = field(default_factory=dict)

    def _metadata(self):
        return _metadata(self.metadata)

    def to_contract(self) -> CapabilityContract:
        return CapabilityContract(
            call_static(
                "AIKernel.Core.Vfs.VfsGit.VfsGitCapabilityContracts",
                "AIKernel.Core",
                "ToContract",
                create_managed(
                    "AIKernel.Core.Vfs.VfsGit.VfsGitCapabilityDescriptor",
                    "AIKernel.Core",
                    self.capability_id,
                    self.repository_mode,
                    self._metadata(),
                ),
            )
        )


def _metadata(values: Mapping[str, str]):
    metadata = {"version": "0.1.1"}
    metadata.update({str(key): str(value) for key, value in values.items()})
    return to_dictionary(metadata)
