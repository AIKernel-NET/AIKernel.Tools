# aikernel-tools

[English](README.md)

AIKernel.Tools の公開 instrumentation surface を扱う Python wrapper です。

この package は AIKernel.Tools の managed assembly を同梱し、pythonnet 経由で
読み込みます。Python 側では replay facade、inspection facade、chat history の
canonical formatting、公開 capability contract descriptor を単一 API として扱えます。
内部 C# semantics は再実装せず、公開契約境界だけを Python に見せます。

package scope、managed assembly bundle requirements、Linux CoreCLR loading、
validation guidance は [Python Tools Wrapper](../docs/python/index-ja.md) を参照してください。

## インストール

```bash
pip install aikernel-tools
```

## 使用例

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

`ReplayEngine`、`Inspector`、`CanonicalFormatter` は、同名 method を持つ backend
object に委譲できます。`MdExporter`、`RomExporter`、`ChatHistoryScraper`、
`NowCommand` / `InfoCommand` などの inspector command facade は public C# 名を
mirror するため、Python caller も同じ contract surface を扱えます。これにより
Python は契約境界に留まり、実装は設定された tooling runtime 側に保持されます。

wheel は `AIKernel.Tools.Instrumentation.dll`、
`AIKernel.Tools.Capability.RomStorage.dll`、inspector assemblies、AIKernel.NET
contract assemblies、AIKernel.Core dependencies、`ChatHistoryProvider.dll` を同梱します。
