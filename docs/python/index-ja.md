# Python Tools Wrapper

[English](index.md)

`aikernel-tools` は public AIKernel.Tools instrumentation surface の Python wrapper
です。

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

package は次を公開します。

- replay / inspection facade
- canonical chat-history formatting
- C# named chat-history scraper / exporter facade
- C# named KernelClock / VFS inspector command facade
- public Capability contract wrapper
- managed assembly discovery と pythonnet loading

Python で Tools internals を再実装しません。managed assembly は
`aikernel_tools/native` に同梱し、pythonnet / CoreCLR で読み込みます。

## Managed Assembly Bundle

wheel には Tools assemblies と contract dependencies を含める必要があります。

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

Linux validation では pythonnet が CoreCLR を load できる必要があります。loader は
managed reference を追加する前に `pythonnet.load("coreclr")` を明示的に呼び出します。

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

source check の推奨コマンドです。

```bash
python -m pytest
python -m compileall src tests
```

Linux wheel validation では共有 Docker test image と virtual environment を使います。
base image は PEP 668 に従うため、system-wide pip installation は block されます。

## C# Surface Mapping

Python package は public C# package surface を mirror します。

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

Python wrapper は thin wrapper のままにしてください。behavior が C# にある場合、
Python は managed assembly に委譲するか managed contract の data wrapper を公開します。
replay、canonical formatting、ROM serialization semantics を Python 側で別解釈しません。

## Publication Checklist

wheel 公開前の check list です。

- `aikernel_tools/native` に最新 Release DLL が入っていることを確認する
- `python/dist` から stale wheel を削除する
- `py -m compileall src tests` を実行する
- `py -m pytest` を実行する
- `py -m build` を実行する
- generated wheel metadata に project URLs、license、pythonnet dependency、bundled
  managed assemblies が含まれることを確認する

wheel は PyPI から見ると pure Python ですが managed assembly を同梱します。Linux host
では pythonnet が利用できる CoreCLR runtime が必要です。
