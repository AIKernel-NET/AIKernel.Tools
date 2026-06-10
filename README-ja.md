# AIKernel.Tools

[English README](README.md)

AIKernel.Tools は、AIKernel の公式 tools、CLI、inspectors、instrumentation
のためのワークスペースです。

AIKernel.Tools は、2026-06-09 に予定している AIKernel 0.1.1 prototype
validation phase に参加します。公開済みの AIKernel.NET contract packages と
AIKernel.Core runtime を、instrumentation utility と developer tools から検証します。

## 目的

AIKernel.Tools は次を提供します。

- AIKernel のデバッグと tooling を拡張する instrumentation helper
- Replay、Snapshot、ROM、Context Assembly workflow のための utility
- Codex、ChatGPT、user-land pipeline から利用できる外部 tool
- 将来の `aik` CLI の基盤
- dynamically loaded external provider の manifest discovery
- ILA（Interface-Led Architecture）に沿った extension point

## リポジトリ境界

Tools は AIKernel.Core の外側にあります。Tools は operational utility と
instrumentation Capability module を所有しますが、Core runtime behavior、provider
implementation、contract definition は所有しません。

Control-plane execution engine は AIKernel.Control に属します。Demo project は
Tools、Control、Providers を利用する側であり、runtime を定義しません。

AIKernel.Tools は Semantic Runtime の operator / instrumentation layer です。
Core、Control、Providers と並べて install されることを想定しますが、diagnostics
や CLI operation が不要な production host からは安全に外せる位置づけです。
runtime contract は AIKernel.NET と AIKernel.Core、physical execution は
AIKernel.Control、provider-specific driver は AIKernel.Providers が所有します。

## Quick Start

CLI tool を install し、最小の 4 つの check を実行します。

```bash
dotnet tool install -g AIKernel.Tools.CLI

aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

これにより、CLI が install 済みであること、Core runtime が応答すること、
VFS boundary が current directory を inspect できること、標準の
`<module> <operation>` 形式で capability module を invoke できることを確認できます。

## 0.1.1 Repository Layout

共通 project property は `Directory.Build.props` に集約されています。

- `src/AIKernel.CLI` - `aik` command-line entry point
- `src/AIKernel.Tools.Capability.RomStorage` - Core-owned ROM storage
  contract 向けの薄い bridge
- `src/AIKernel.Tools.Instrumentation` - deterministic debugging
  のための replay、inspector、canonical formatting primitive
- `src/AIKernel.Tools.Inspectors.KernelClock` - deterministic Kernel clock inspector
- `src/AIKernel.Tools.Inspectors.Vfs` - VFS inspector
- `src/AIKernel.Tools.Inspectors.ChatHistoryScraper` - ChatGPT shared-history
  scraper と HistoryROM exporter

## Capability Contract Alignment

Capability module は local な実装 descriptor を持ってよいですが、登録境界は
`AIKernel.Dtos.Capabilities` の共有 contract に揃えます。

次の module は `CapabilityModuleDescriptor` への mapper を提供します。

- `AIKernel.Core.Storage`
- `AIKernel.Core.Vfs.VfsGit`

これらの module は Control Plane interface を所有せず、routing request を直接
変更しません。Control contract は `AIKernel.Abstractions.Control` と
`AIKernel.Dtos.Control` が所有します。provider-routing decision は
`AIKernel.Dtos.Routing` の pure DTO であり、Core runtime helper が適用します。

0.1.1 の Tools package family は capability ownership を意図的に狭く保ちます。
以前 Tools 配下にあった provider-oriented module は AIKernel.Providers へ移管済み
です。Core-owned ROM/VFS contract は AIKernel.Core に残り、Tools はそれらを
inspect、invoke、export するための compatibility bridge と operator command のみを
保持します。

## Public Package Surface

この repository の public package set は次の通りです。

- `aik` - command-line tool package
- `AIKernel.Tools.Instrumentation` - replay、inspection、canonical formatting、
  deterministic serialization primitive
- `AIKernel.Tools.Capability.RomStorage` - Core-owned ROM storage contract 向けの
  compatibility bridge
- `AIKernel.Tools.Inspectors.ChatHistoryScraper` - shared-history extraction と
  deterministic Markdown / HistoryROM export
- `AIKernel.Tools.Inspectors.KernelClock` - clock snapshot / timeline diagnostic
  command surface
- `AIKernel.Tools.Inspectors.Vfs` - bounded local VFS diagnostic command surface
- `aikernel-tools` - 同じ public C# tooling surface を扱う Python wrapper

package set には OpenAI、CUDA、Local LLM、Dynamic Pipeline Compiler、VFS Git の
provider implementation は含まれません。それらは AIKernel.Providers または
AIKernel.Core が所有し、manifest または Core standard provider 経由で操作します。

## ドキュメント

- [Documentation index](docs/README-ja.md)
- [User Guide](docs/user-guide/index-ja.md)
- [Architecture](docs/architecture/index-ja.md)
- [CLI operations](docs/cli/index-ja.md)
- [Capability modules](docs/capabilities/index-ja.md)
- [Instrumentation](docs/instrumentation/index-ja.md)
- [Inspectors](docs/inspectors/index-ja.md)
- [Tool pipelines](docs/pipelines/index-ja.md)
- [Python tools wrapper](docs/python/index-ja.md)
- [Licensing](docs/licensing/index-ja.md)

## ビルド

```powershell
dotnet build AIKernel.Tools.slnx
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- --help
```

公開前の推奨 validation です。

```powershell
dotnet build AIKernel.Tools.slnx -c Release --no-restore
dotnet test AIKernel.Tools.slnx -c Release --no-build
dotnet pack AIKernel.Tools.slnx -c Release --no-build -o artifacts/packages
cd python
py -m compileall src tests
py -m pytest
py -m build
```

NuGet package は default では unsigned です。repository policy として signed
package を要求する場合は、upload 前に signing step を追加してください。

## CLI Smoke Commands

```powershell
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- vfs tree .
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- vfs info .
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- clock now
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- clock timeline
```

## Operational CLI Commands

`aik` command は Linux 風の subcommand として整理しています。

```bash
aik runtime ping
aik system info
aik system providers
aik system capabilities
aik capabilities list
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
aik exec run pipeline.json input.text=hello
aik skills list --root ./skills
aik skills show skill.example --root ./skills
aik skills invoke skill.example --root ./skills text=hello
aik providers list --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
aik gpu list
aik gpu run vector-add --a a.bin --b b.bin
aik run sample
aik ps
aik kill <pid-or-name>
aik restart <pid-or-name>
aik logs sample
aik schedule add --every 1m "aik system info"
```

standard provider command は、実行前に Core standard providers を初期化します。
対象は MinimalRuntimeProvider、SystemInfoProvider、VfsProvider、
LocalExecutionProvider、SkillProvider です。external provider command は従来通り
`aik providers` 配下で provider manifest を読み込みます。

SkillProvider は `SKILL.md` と `Skill.MD` の両方を再帰的に discover します。
public な主表記としては `SKILL.md` を推奨します。

external provider は deterministic な manifest file から load されます。最小例は
次の通りです。

```json
{
  "id": "openai.chat",
  "name": "OpenAI Chat Provider",
  "version": "0.1.1",
  "assembly": "AIKernel.Providers.OpenAI.dll",
  "capabilities": [
    "chat.completion"
  ]
}
```

## パッケージインストール

.NET CLI tool として install する場合:

```bash
dotnet tool install -g AIKernel.Tools.CLI
```

.NET host では NuGet package を使用します。

```bash
dotnet add package AIKernel.Tools.Instrumentation --version 0.1.1
dotnet add package AIKernel.Tools.Capability.RomStorage --version 0.1.1
dotnet add package AIKernel.Tools.Inspectors.ChatHistoryScraper --version 0.1.1
dotnet add package AIKernel.Tools.Inspectors.KernelClock --version 0.1.1
dotnet add package AIKernel.Tools.Inspectors.Vfs --version 0.1.1
```

Python host では PyPI package を使用します。

```bash
pip install aikernel-tools
```

Python module は `aikernel_tools` として import します。

```python
from aikernel_tools import CanonicalFormatter, ChatHistoryRecord
```

wheel は managed AIKernel.Tools assemblies を `aikernel_tools/native` に同梱します。
これは public C# contract surface への wrapper であり、tooling semantics を
Python で別実装するものではありません。

Python wrapper は `ReplayEngine`、`ReplaySession`、`Inspector`、
`CanonicalFormatter`、`CanonicalSerializer`、`MdExporter`、`RomExporter`、
`ChatHistoryScraper`、KernelClock/VFS inspector command facade、public Capability
contract wrapper を公開します。Python caller はこれらを managed contract facade と
して扱い、同梱 assembly の private implementation detail に依存しないでください。

## コントリビュータ向けガイドライン

Tools と CLI の変更は、AIKernel 共通の開発規律に従ってください。

- [AIKernel 開発ガイドライン](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES-jp.md)
- [AIKernel Development Guidelines](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES.md)

CLI command は failure を deterministic な exit code へ変換し、command boundary
の外へ implementation exception を漏らさず、instrumentation を pure かつ
reproducible に保ち、Python wrapper を public C# tooling contract と整合させてください。

## ライセンス

Apache License 2.0.
