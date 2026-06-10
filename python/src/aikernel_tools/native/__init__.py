"""[EN]
Managed assembly discovery and pythonnet loading for AIKernel.Tools.

[JA]
AIKernel.Tools の managed assembly 探索と pythonnet 読み込みを提供します。
"""

from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path


_TOOLS_PACKAGE_VERSION = "0.1.1"
_CONTRACT_PACKAGE_VERSION = "0.1.1"
_CORE_PACKAGE_VERSION = "0.1.1"
_PROVIDER_PACKAGE_VERSION = "0.1.1"
_ASSEMBLIES = (
    "AIKernel.Abstractions.dll",
    "AIKernel.Common.dll",
    "AIKernel.Core.dll",
    "AIKernel.Dtos.dll",
    "AIKernel.Enums.dll",
    "ChatHistoryProvider.dll",
    "AIKernel.Tools.Instrumentation.dll",
    "AIKernel.Tools.Capability.RomStorage.dll",
    "AIKernel.Tools.Inspectors.ChatHistoryScraper.dll",
    "AIKernel.Tools.Inspectors.KernelClock.dll",
    "AIKernel.Tools.Inspectors.Vfs.dll",
)
_ASSEMBLY_PACKAGES = {
    "AIKernel.Abstractions.dll": ("AIKernel.Abstractions", _CONTRACT_PACKAGE_VERSION),
    "AIKernel.Common.dll": ("AIKernel.Common", _CORE_PACKAGE_VERSION),
    "AIKernel.Core.dll": ("AIKernel.Core", _CORE_PACKAGE_VERSION),
    "AIKernel.Dtos.dll": ("AIKernel.Dtos", _CONTRACT_PACKAGE_VERSION),
    "AIKernel.Enums.dll": ("AIKernel.Enums", _CONTRACT_PACKAGE_VERSION),
    "ChatHistoryProvider.dll": ("AIKernel.Providers.ChatHistory", _PROVIDER_PACKAGE_VERSION),
    "AIKernel.Tools.Instrumentation.dll": (
        "AIKernel.Tools.Instrumentation",
        _TOOLS_PACKAGE_VERSION,
    ),
    "AIKernel.Tools.Capability.RomStorage.dll": (
        "AIKernel.Tools.Capability.RomStorage",
        _TOOLS_PACKAGE_VERSION,
    ),
    "AIKernel.Tools.Inspectors.ChatHistoryScraper.dll": (
        "AIKernel.Tools.Inspectors.ChatHistoryScraper",
        _TOOLS_PACKAGE_VERSION,
    ),
    "AIKernel.Tools.Inspectors.KernelClock.dll": (
        "AIKernel.Tools.Inspectors.KernelClock",
        _TOOLS_PACKAGE_VERSION,
    ),
    "AIKernel.Tools.Inspectors.Vfs.dll": (
        "AIKernel.Tools.Inspectors.Vfs",
        _TOOLS_PACKAGE_VERSION,
    ),
}


@dataclass(frozen=True)
class ToolsAssemblySet:
    """[EN]
    Resolved managed assemblies that define the Tools instrumentation boundary.

    [JA]
    Tools instrumentation 境界を定義する解決済み managed assembly 群です。
    """

    root: Path
    assemblies: tuple[Path, ...]

    @property
    def is_complete(self) -> bool:
        """[EN] Return whether every expected assembly exists.

        [JA] 期待されるすべての assembly が存在するかを返します。
        """
        return all(path.exists() for path in self.assemblies)

    @property
    def missing(self) -> tuple[str, ...]:
        """[EN] Return missing assembly file names.

        [JA] 不足している assembly ファイル名を返します。
        """
        return tuple(path.name for path in self.assemblies if not path.exists())


def tools_assemblies() -> ToolsAssemblySet:
    """[EN] Resolve bundled or NuGet-provided Tools assemblies.

    [JA] 同梱または NuGet 提供の Tools assembly を解決します。
    """
    root = _native_root()
    return ToolsAssemblySet(
        root=root,
        assemblies=tuple(_resolve_assembly(name) for name in _ASSEMBLIES),
    )


def require_tools_assemblies() -> ToolsAssemblySet:
    """[EN] Resolve assemblies and fail if any required assembly is missing.

    [JA] assembly を解決し、不足がある場合は失敗します。
    """
    assemblies = tools_assemblies()
    if not assemblies.is_complete:
        missing = ", ".join(assemblies.missing)
        raise FileNotFoundError(
            "AIKernel.Tools managed assemblies are not available: "
            f"{missing}. Bundle them in aikernel_tools/native or restore "
            "the corresponding NuGet packages."
        )
    return assemblies


def load_tools_runtime() -> ToolsAssemblySet:
    """[EN] Load bundled C# assemblies through pythonnet.

    [JA] 同梱 C# assembly を pythonnet 経由で読み込みます。
    """
    assemblies = require_tools_assemblies()
    try:
        from pythonnet import load  # type: ignore[import-not-found]

        try:
            load("coreclr")
        except RuntimeError:
            # The runtime may already be loaded by another AIKernel wrapper.
            pass
        import clr  # type: ignore[import-not-found]
    except ImportError as exc:
        raise RuntimeError("pythonnet is required to load AIKernel.Tools assemblies.") from exc

    for assembly in assemblies.assemblies:
        clr.AddReference(str(assembly))
    return assemblies


def _resolve_assembly(name: str) -> Path:
    for root in _assembly_roots():
        candidate = root / name
        if candidate.exists():
            return candidate

    nuget_candidate = _resolve_nuget_assembly(name)
    if nuget_candidate is not None:
        return nuget_candidate

    return _native_root() / name


def _assembly_roots() -> tuple[Path, ...]:
    roots = [_native_root()]
    override = os.environ.get("AIKERNEL_TOOLS_ASSEMBLY_PATH")
    if override:
        roots.extend(Path(path) for path in override.split(os.pathsep) if path)
    return tuple(roots)


def _native_root() -> Path:
    return Path(__file__).resolve().parent


def _resolve_nuget_assembly(name: str) -> Path | None:
    package, version = _ASSEMBLY_PACKAGES[name]
    candidate = _nuget_root() / package.lower() / version / "lib" / "net10.0" / name
    if candidate.exists():
        return candidate
    return None


def _nuget_root() -> Path:
    configured = os.environ.get("NUGET_PACKAGES")
    if configured:
        return Path(configured)
    return Path.home() / ".nuget" / "packages"
