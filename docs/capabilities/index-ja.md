# Tool Capability Modules

[English](index.md)

Tools は AIKernel pipeline のための instrumentation Capability module を公開できます。
ただし provider-specific module は AIKernel.Providers または別の provider
repository に置き、CLI / Core registry が manifest から読み込みます。

0.1.1 の Tools capability focus は意図的に狭くしています。

- `AIKernel.Tools.Capability.RomStorage` - Core-owned ROM storage contract 向けの
  compatibility bridge
  - Provides: `rom.save`, `rom.load`, `rom.list`
  - Demo usage: CapabilityROM ecosystem demos と ROM-backed recomputation

VFS Git contract は `AIKernel.Core.Vfs.VfsGit` が所有します。

Capability module と external provider は contract definition を重複定義せず、
AIKernel.NET contracts と AIKernel.Core runtime packages を利用してください。

Inspector は観測と debugging のために分離します。既定では pipeline-callable
Capability function を定義しません。

## Standard Capability Operations

`aik` CLI は、Core standard Capability module を Tools 側へ移さずに操作できます。

```bash
aik capabilities list
aik capabilities invoke aikernel.runtime.ping runtime.ping
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
aik capabilities invoke aikernel.system.info system.info
```

standard module は AIKernel.Core standard providers によって登録されます。

- MinimalRuntimeProvider: `aikernel.runtime.ping`
- SystemInfoProvider: `aikernel.system.info`
- VfsProvider: `aikernel.vfs`
- LocalExecutionProvider: `aikernel.local.execute`
- SkillProvider: `SKILL.md`-derived `skill.*` capabilities

Tools が所有するのは、これらを運用する command surface であり、provider
implementation ではありません。

## External Provider Boundary

external provider module は Tools の外側に置きます。

```bash
aik install provider dynamic-pipeline
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

これにより OpenAI、CUDA、Local LLM、Dynamic Pipeline Compiler、ChatHistory の
provider logic を AIKernel.Providers に保持しつつ、operator command surface は
ひとつに保てます。

## ROM Storage Bridge

`AIKernel.Tools.Capability.RomStorage` は package-level の薄い compatibility bridge
としてのみ存在します。ROM storage contract 自体は 0.1.0.2 の Core patch line 以降、
`AIKernel.Core.Storage` が所有します。Tools は Tools-family package を解決したい
既存 tooling workflow のために NuGet package と Python bridge を提供しますが、
descriptor creation は Core contract に委譲します。

bridge は次を行いません。

- 2 つ目の ROM storage semantics を定義する
- ChatHistory provider implementation code を所有する
- provider manifest を生成する
- Core routing や Control execution request を mutate する

bridge は次を行えます。

- Core contract 経由で `CapabilityModuleDescriptor` を生成する
- CLI / Python tooling に必要な metadata を公開する
- deterministic かつ side-effect free に保つ

## Descriptor Shape

Tools 経由で公開する capability descriptor は invocation surface のみを記述します。

- capability id
- display name
- kind
- invocation mode
- supported operations
- required permissions
- artifact URI / hash
- ordinally stable dictionary としての metadata

HTTP endpoint、model name、CUDA runtime path、provider secret などの implementation
detail は Tools capability descriptor ではなく AIKernel.Providers manifest に属します。

## Validation

capability package 変更時の推奨 check です。

```powershell
dotnet build AIKernel.Tools.slnx -c Release --no-restore
dotnet test AIKernel.Tools.slnx -c Release --no-build
dotnet pack AIKernel.Tools.slnx -c Release --no-build -o artifacts/packages
```

Python alignment の確認です。

```powershell
cd python
py -m pytest
py -m compileall src tests
```

AIKernel.Providers へ移管済みの provider 用 `AIKernel.Tools.Capability.*` package を
stale artifact として emit しないでください。
