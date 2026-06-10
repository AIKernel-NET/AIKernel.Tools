# Python Tools Wrapper

[日本語](index-ja.md)

`aikernel-tools` is the Python wrapper for the public AIKernel.Tools
instrumentation surface.

Install:

```bash
pip install aikernel-tools
```

Import:

```python
from aikernel_tools import (
    CanonicalFormatter,
    ChatHistoryRecord,
    ChatHistoryScraper,
    InfoCommand,
    MdExporter,
    NowCommand,
    ReplayEngine,
    Inspector,
    RomExporter,
    RomStorageCapability,
    TimelineCommand,
    TreeCommand,
    VfsGitCapability,
    tools_assemblies,
)
```

## Package Scope

The package exposes:

- replay and inspection facades
- canonical chat-history formatting
- C#-named chat-history scraper/exporter facades
- C#-named KernelClock and VFS inspector command facades
- public Capability contract wrappers
- managed assembly discovery and pythonnet loading

It does not reimplement Tools internals in Python. Managed assemblies are
bundled under `aikernel_tools/native` and loaded with pythonnet/CoreCLR.

## Managed Assembly Bundle

The wheel must include Tools assemblies and their contract dependencies:

- `AIKernel.Abstractions.dll`
- `AIKernel.Common.dll`
- `AIKernel.Core.dll`
- `AIKernel.Dtos.dll`
- `AIKernel.Enums.dll`
- `AIKernel.Tools.Instrumentation.dll`
- `AIKernel.Tools.Capability.RomStorage.dll`
- `AIKernel.Tools.Inspectors.ChatHistoryScraper.dll`
- `AIKernel.Tools.Inspectors.KernelClock.dll`
- `AIKernel.Tools.Inspectors.Vfs.dll`
- `ChatHistoryProvider.dll`

Linux validation requires pythonnet to load CoreCLR. The loader explicitly calls
`pythonnet.load("coreclr")` before adding managed references.

## Example

```python
from aikernel_tools import CanonicalFormatter, ChatHistoryRecord, MdExporter, RomExporter

formatter = CanonicalFormatter()
rom = formatter.serialize([
    ChatHistoryRecord("user", "hello", "2026-06-09T00:00:00+00:00")
])
markdown = MdExporter.to_markdown([
    ChatHistoryRecord("assistant", "world", "2026-06-09T00:00:01+00:00")
])
history_rom = RomExporter.to_rom([
    ChatHistoryRecord("assistant", "world", "2026-06-09T00:00:01+00:00")
])
print(rom)
```

## Validation

Recommended source checks:

```bash
python -m pytest
python -m compileall src tests
```

For Linux wheel validation, use the shared Docker test image and a virtual
environment because the base image follows PEP 668 and blocks system-wide pip
installation.

## C# Surface Mapping

The Python package mirrors the public C# package surface:

- `ReplayEngine` -> `AIKernel.Tools.Instrumentation.ReplayEngine`
- `ReplaySession` -> `AIKernel.Tools.Instrumentation.ReplaySession`
- `Inspector` -> `AIKernel.Tools.Instrumentation.Inspector`
- `CanonicalFormatter` -> `AIKernel.Tools.Instrumentation.CanonicalFormatter`
- `CanonicalSerializer` -> `AIKernel.Tools.Instrumentation.CanonicalSerializer`
- `MdExporter` -> `AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.MdExporter`
- `RomExporter` -> `AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.RomExporter`
- `ChatHistoryScraper` -> `AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper`
- `NowCommand` / `TimelineCommand` -> KernelClock inspector commands
- `InfoCommand` / `TreeCommand` -> VFS inspector commands
- `RomStorageCapability` -> Core-owned ROM storage descriptor bridge
- `VfsGitCapability` -> Core-owned VFS Git descriptor bridge

Python wrappers must remain thin. If a behavior exists in C#, Python should
delegate to the managed assembly or expose a data wrapper over the managed
contract. Python should not introduce a separate interpretation of replay,
canonical formatting, or ROM serialization semantics.

## Publication Checklist

Before publishing the wheel:

- ensure `aikernel_tools/native` contains the latest Release DLLs
- remove stale wheels from `python/dist`
- run `py -m compileall src tests`
- run `py -m pytest`
- run `py -m build`
- inspect the generated wheel metadata for project URLs, license, pythonnet
  dependency, and bundled managed assemblies

The wheel is pure Python from PyPI's perspective but carries managed assemblies.
Linux hosts require a usable CoreCLR runtime for pythonnet.
