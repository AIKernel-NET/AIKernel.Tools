# aikernel-tools

[日本語](README-ja.md)

Python wrapper for the public instrumentation surface of AIKernel.Tools.

The package bundles the AIKernel.Tools managed assemblies and loads them through
pythonnet. Python code receives a single API surface for replay facades,
inspection facades, canonical chat-history formatting, and public capability
contract descriptors without re-implementing the internal C# semantics.

See [Python Tools Wrapper](../docs/python/index.md) for package scope, managed
assembly bundle requirements, Linux CoreCLR loading, and validation guidance.

## Install

```bash
pip install aikernel-tools
```

## Usage

```python
from aikernel_tools import (
    CanonicalFormatter,
    ChatHistoryRecord,
    InfoCommand,
    MdExporter,
    NowCommand,
    RomExporter,
    RomStorageCapability,
)

capability = RomStorageCapability("tools.rom")
contract = capability.to_contract()

formatter = CanonicalFormatter()
rom = formatter.serialize([
    ChatHistoryRecord("user", "hello", "2026-06-09T00:00:00+00:00")
])

markdown = MdExporter.to_markdown([
    ChatHistoryRecord("assistant", "world", "2026-06-09T00:00:01+00:00")
])
history_rom = RomExporter.to_rom(
    [ChatHistoryRecord("assistant", "world", "2026-06-09T00:00:01+00:00")],
    namespace="python",
    name="history",
)
```

`ReplayEngine`, `Inspector`, and `CanonicalFormatter` can also delegate to an
explicit backend object that exposes the same method names. `MdExporter`,
`RomExporter`, `ChatHistoryScraper`, and the inspector command facades such as
`NowCommand` / `InfoCommand` mirror the public C# names so Python callers can
address the same contract surface. This keeps Python as a contract boundary
while the implementation stays in the configured tooling runtime.

The wheel bundles `AIKernel.Tools.Instrumentation.dll`,
`AIKernel.Tools.Capability.RomStorage.dll`, inspector assemblies, AIKernel.NET
contract assemblies, AIKernel.Core dependencies, and `ChatHistoryProvider.dll`.
