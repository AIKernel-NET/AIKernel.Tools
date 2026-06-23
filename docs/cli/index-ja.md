# AIKernel Tools CLI

[English](index.md)

`aik` は AIKernel.Tools の operator command です。Linux 風の subcommand 形式で
利用します。

```bash
aik <command> [arguments]
```

command surface は、standard Core providers と external provider manifests に
分かれます。

## Quick Start

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.3

aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

これにより install、runtime health、VFS access、capability invocation を最小手順で
確認できます。

## Installation

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.3
```

## Standard Provider Commands

これらの command は実行前に AIKernel.Core standard providers を初期化します。

- MinimalRuntimeProvider
- SystemInfoProvider
- VfsProvider
- LocalExecutionProvider
- SkillProvider

Commands:

```bash
aik runtime ping
aik system info
aik system providers
aik system capabilities
aik system vfs --vfs-root .
aik system runtime
aik capabilities list
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
aik exec run pipeline.json input.text=hello
aik skills list --root ./skills
aik skills show skill.example --root ./skills
aik skills invoke skill.example --root ./skills text=hello
```

CLI は invocation では deterministic key/value output を、system snapshot では
compact JSON を出力します。これにより shell script、ReplayLog comparison、CI
diagnostics で扱いやすくなります。

## External Provider Commands

external provider implementation は Tools に含めません。provider manifest から
install / load します。

```bash
aik install provider dynamic-pipeline
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

これにより provider implementation logic を AIKernel.Providers に保持しつつ、
operator は単一の command-line tool を利用できます。

## OS Commands

CLI は compute と process operation を Linux 風の OS surface として公開します。

```bash
aik gpu list
aik gpu verify rev3
aik gpu pending rev3
aik gpu verify-native --provider dawn --package AIKernel.Dawn.Provider.0.1.3-dev1.nupkg
aik gpu verify-native --provider cuda13 --package AIKernel.Cuda13.0.Libtorch2.12.win-x64.0.1.3-dev1.nupkg
aik gpu verify-native --provider cuda13 --library native/build/win-x64/Release/libtorch_bridge.dll
aik gpu run vector-add --a a.bin --b b.bin
aik run sample
aik ps
aik kill <pid-or-name>
aik restart <pid-or-name>
aik logs sample
aik schedule add --every 1m "aik system info"
```

`aik gpu verify rev3` は canonical GPU layout、metadata、diagnostics、
provider boundary rule を検証します。
`aik gpu pending rev3` は canonical rev3 lane の優先度付き残作業 table を出力します。
`aik gpu verify-native --provider dawn` は DawnCore runtime asset と、
library 指定時の native export を検証します。
`aik gpu verify-native --provider cuda13` は LibTorch/CUDA runtime の load を要求せず、
package loader と `libtorch_bridge.dll` runtime asset を検証します。
Cuda13 library 指定時は `aikernel_cuda13_dispatch` を load し、
canonical 40-byte staged dispatch header を送信して、
`NotInitialized` / `CommandSubmissionDisabled` と invalid-length の fail-closed 経路を検証します。

この workspace で build 済みの Dawn native fixture を検証する場合は、local-feed
helper を使用してください。

```powershell
.\scripts\verify-dawn-native-fixture.ps1
```

この helper は `../artifacts/NuGet.rev3-local.v0.1.3-dev1.config` を生成し、
`../artifacts/local-nuget/v0.1.3-dev1` に対して CLI を restore/build した上で
`aik gpu verify-native --provider dawn --library <custom_bridge>` を実行します。
これにより、local canonical rev3 smoke check 中に未公開の `0.1.3` package を
NuGet.org から解決しようとする事故を避けられます。

rev3 GPU lane 全体を local でまとめて検証する場合は、次を使用します。

```powershell
.\scripts\verify-rev3-gpu-local-lane.ps1
```

この lane は Tools smoke test、Wasm WebGPU package smoke、Dawn package / fixture
check、Cuda13 package smoke、Cuda13 direct-library probe を順に実行します。
Cuda13 direct-library probe は依存 DLL を考慮し、`-RequireCuda13LibraryLoad` を
指定しない限り現在の missing-dependent-module 境界を許容します。
release lane では `-RequireFreshCuda13NativeBridge` を指定すると、packaged
`libtorch_bridge.dll` が native C++ bridge source より古い場合に拒否できます。
この lane は生成済み rev3 local NuGet config も Dawn/Cuda13 の CLI build step に
渡すため、未公開 `0.1.3-dev*` 依存関係の解決経路を揃えられます。

公開済み `0.1.3` package がまだ無い状態で ad hoc command を実行する場合は、
local NuGet config を生成し、明示的に渡します。

```powershell
.\scripts\new-rev3-local-nuget-config.ps1
dotnet restore src/AIKernel.CLI/AIKernel.CLI.csproj --configfile ..\artifacts\NuGet.rev3-local.v0.1.3-dev1.config -p:UseLocalPackageVersion=true -p:LocalPackageBuildNumber=1
dotnet run --no-restore --project src/AIKernel.CLI/AIKernel.CLI.csproj -p:UseLocalPackageVersion=true -p:LocalPackageBuildNumber=1 -- gpu pending rev3
```

## VFS Root

standard VFS capability を読む command は次を受け取ります。

```bash
--vfs-root <path>
```

既定値は current directory です。CLI はこの root path に read-only local file
provider を登録するため、`vfs.exists`、`vfs.read_file`、`vfs.list`、
`vfs.metadata` を外部サービスなしで実行できます。

## Skill Root

`SKILL.md` を読み込む command は次を受け取ります。

```bash
--root <path>
--skill-root <path>
```

root の既定値は `skills` または `AIKERNEL_SKILL_ROOT` environment variable です。
SkillProvider は `SKILL.md` と `Skill.MD` を再帰的に発見し、capability module
として登録します。public な主表記としては `SKILL.md` を推奨します。

## Provider Manifest Example

external provider は manifest file から discover されます。最小例は次の通りです。

```json
{
  "id": "openai.chat",
  "name": "OpenAI Chat Provider",
  "version": "0.1.3",
  "assembly": "AIKernel.Providers.OpenAI.dll",
  "capabilities": [
    "chat.completion"
  ]
}
```

## Failure Behavior

CLI command は fail-closed です。

- unknown command は non-zero exit code を返します。
- unsupported capability operation は provider error envelope を返します。
- provider manifest や `SKILL.md` root が無い場合、provider logic を黙って生成しません。
- external provider command failure は provider registry の error message として表示します。

運用上の失敗を見える形で deterministic に扱うことが目的です。

## Command Families

CLI は責務ごとに command family を分離します。

- `runtime` は minimal runtime boundary を確認します。
- `system` は安全な introspection metadata を読み取ります。
- `capabilities` は `aik capabilities invoke <module> <operation> key=value` 形式で
  Core standard capability module を操作します。
- `exec` は local DSL pipeline を LocalExecutionProvider に渡します。
- `skills` は `SKILL.md`-derived capability を discover / invoke します。
- `providers` は external provider manifest を読み込み、その capability module を
  invoke します。
- `install provider` は build 済み provider manifest と assembly を local provider
  directory に配置します。
- `clock`、`vfs`、`rom` は operator diagnostics と compatibility workflow 向けの
  direct inspector command です。
- `nomos` は ROM / Canon inspection の非破壊 alias です。
- `chronos` と `replay` は timeline inspection の非破壊 alias です。

command 名は UI 風の表現ではなく、Linux-style の verb / subcommand を意図して
採用しています。

concept alias は追加のみです。

```bash
aik rom view
aik nomos view
aik clock timeline
aik chronos timeline
aik replay timeline
```

## Exit Codes

command は成功時に `0` を返します。次の場合は non-zero を返します。

- command または subcommand が不明
- 必須 argument が不足している
- provider manifest が見つからない、または読み込めない
- requested capability に対応する invoker が登録されていない
- invoked capability が error envelope を返した

failure 時も output は parseable に保ちます。CI log と replay diagnostics が失敗境界を
捕捉できるよう、error text は明示的に出力します。

## Publication Checks

CLI package 公開前に次を確認します。

- `dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- --help`
- `aik runtime ping`
- `aik system info`
- `aik capabilities list`
- `AIKernel.Tools.Tests` の smoke tests
- temporary stub text を command が出力しないこと
