# Tool Capability Modules

[日本語](index-ja.md)

Tools may expose instrumentation Capability modules for AIKernel pipelines, but
provider-specific modules now live in AIKernel.Providers or another provider
repository and are loaded by manifest through the CLI/Core registry.

The 0.1.1 Tools capability focus is intentionally narrow:

- `AIKernel.Tools.Capability.RomStorage` - compatibility bridge for the
  Core-owned ROM storage contract.
  - Provides: `rom.save`, `rom.load`, `rom.list`.
  - Demo usage: CapabilityROM ecosystem demos and ROM-backed recomputation.

VFS Git contracts are owned by `AIKernel.Core.Vfs.VfsGit`.

Capability modules and external providers should use AIKernel.NET contracts and
AIKernel.Core runtime packages rather than duplicating contract definitions.

Inspectors remain separate because they observe and debug; they do not define
pipeline-callable Capability functions by default.

## Standard Capability Operations

The `aik` CLI can operate Core standard Capability modules without moving them
into Tools:

```bash
aik capabilities list
aik capabilities invoke aikernel.runtime.ping runtime.ping
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
aik capabilities invoke aikernel.system.info system.info
```

The standard modules are registered by AIKernel.Core standard providers:

- MinimalRuntimeProvider: `aikernel.runtime.ping`
- SystemInfoProvider: `aikernel.system.info`
- VfsProvider: `aikernel.vfs`
- LocalExecutionProvider: `aikernel.local.execute`
- SkillProvider: `SKILL.md`-derived `skill.*` capabilities

Tools owns the command surface for operating these modules, not the provider
implementation.

## External Provider Boundary

External provider modules remain outside Tools. Use:

```bash
aik install provider dynamic-pipeline
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

This keeps OpenAI, CUDA, Local LLM, Dynamic Pipeline Compiler, and ChatHistory
provider logic in AIKernel.Providers while preserving one operator command
surface.

## ROM Storage Bridge

`AIKernel.Tools.Capability.RomStorage` exists only as a thin package-level
compatibility bridge. The ROM storage contract itself is owned by
`AIKernel.Core.Storage` after the 0.1.0.2 Core patch line. Tools exposes a
Python bridge and NuGet package so existing tooling workflows can resolve a
Tools-family package while still delegating descriptor creation to Core.

The bridge must not:

- define a second ROM storage semantics
- own ChatHistory provider implementation code
- write provider manifests
- mutate Core routing or Control execution requests

The bridge may:

- produce `CapabilityModuleDescriptor` values through Core contracts
- expose metadata required by CLI/Python tooling
- remain deterministic and side-effect free

## Descriptor Shape

Capability descriptors exposed through Tools should describe the invocation
surface only:

- capability id
- display name
- kind
- invocation mode
- supported operations
- required permissions
- artifact URI / hash when applicable
- metadata in ordinally stable dictionaries

Implementation details such as HTTP endpoints, model names, CUDA runtime paths,
or provider secrets belong in AIKernel.Providers manifests, not in Tools
capability descriptors.

## Validation

Recommended checks for capability package changes:

```powershell
dotnet build AIKernel.Tools.slnx -c Release --no-restore
dotnet test AIKernel.Tools.slnx -c Release --no-build
dotnet pack AIKernel.Tools.slnx -c Release --no-build -o artifacts/packages
```

For Python alignment:

```powershell
cd python
py -m pytest
py -m compileall src tests
```

The package should not emit stale `AIKernel.Tools.Capability.*` packages for
providers that have moved to AIKernel.Providers.
