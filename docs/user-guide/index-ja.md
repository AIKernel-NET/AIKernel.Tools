# User Guide

[English](index.md)

この guide は、AIKernel.Tools を operator CLI および tooling library として使う
方法を説明します。

Tools は AIOS SDK の observability / instrumentation layer です。AIOS distribution を
operate、inspect、replay、diagnose するために使い、これらの operator surface を
Core や Providers から分離します。

公式 AIOS ディストリビューション **AIKernel.Monolith** の開発も開始されています。
Monolith は 0.1.x 系の安定化後に CLI、replay、inspection、governance-oriented
observability を統合する標準 reference distribution として位置づけられます。

## Install the CLI

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1
```

最小 runtime surface を確認します。

```bash
aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

## Command Families

| Command | Purpose |
| --- | --- |
| `runtime` | minimal runtime health |
| `system` | provider / capability / VFS / runtime の safe introspection |
| `capabilities` | standard capability module の invoke |
| `exec` | local DSL pipeline 実行 |
| `skills` | `SKILL.md` capability の discover / invoke |
| `providers` | external provider manifest の load |
| `gpu` | OS compute command |
| `run`, `ps`, `kill`, `restart` | logical process 管理 |
| `logs` | process log inspection |
| `schedule` | scheduled command 管理 |
| `vfs`, `clock`, `rom` | direct diagnostic inspector |

## Invoke a Capability

standard shape:

```bash
aik capabilities invoke <module> <operation> key=value
```

例:

```bash
aik capabilities invoke aikernel.vfs vfs.exists path=README.md --vfs-root .
```

## Load External Providers

```bash
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

External provider implementation は AIKernel.Providers に残ります。Tools は manifest
を load し、宣言された capability boundary を invoke するだけです。

## Run a Safe Schedule

```bash
aik schedule add --every 1m "aik system info"
```

production-like environment では短い interval を慎重に扱います。

## Use Instrumentation from .NET

```csharp
using AIKernel.Tools.Instrumentation;

var formatter = new CanonicalFormatter();
var canonical = formatter.Format(new { message = "hello" });
```

Instrumentation utility は deterministic で、replay、inspection、test diagnostics
向けです。

## Use Python Wrapper

```python
from aikernel_tools import CanonicalFormatter, ReplayEngine

formatter = CanonicalFormatter()
print(formatter.format({"message": "hello"}))
```

Python package は managed assembly が利用可能な場合に public C# tooling surface へ
委譲します。

## Failure Behavior

Tools command は operator-facing で fail-closed します。

- unknown command は non-zero を返す
- unsupported operation は provider / capability error envelope を返す
- missing manifest は明示的に報告する
- output は script / CI log に適した deterministic さを保つ

## Next Steps

- command example は [CLI Operations](../cli/index-ja.md) を参照してください。
- replay / canonical formatting は [Instrumentation](../instrumentation/index-ja.md) を参照してください。
- wheel 公開前に [Python Wrapper](../python/index-ja.md) を確認してください。
