# AIKernel.Tools

[日本語 README](README-ja.md)

AIKernel.Tools is the official tools, CLI, inspectors, and instrumentation
workspace for AIKernel.

AIKernel.Tools participates in the AIKernel 0.1.1 prototype validation phase
scheduled for 2026-06-10. It validates that instrumentation utilities and
developer tools can consume the published AIKernel.NET contract packages and
AIKernel.Core runtime without owning runtime, provider, or contract definitions.

In the AIOS SDK, AIKernel.Tools is the observability and instrumentation layer.
It provides CLI operations, replay, inspectors, canonical formatting, and
operator workflows that can be installed beside an AIOS distribution and
removed from hosts that do not need diagnostics.

AIKernel also provides an official AIOS distribution, codenamed
**AIKernel.Monolith**. Monolith has begun development as the standard AIOS that
integrates semantic runtime, capability graph, governance, and observability
after the 0.1.x SDK line stabilizes.

## Purpose

AIKernel.Tools provides:

- Instrumentation helpers that extend AIKernel debugging and tooling.
- Utilities for Replay, Snapshot, ROM, and Context Assembly workflows.
- External tools usable from Codex, ChatGPT, and user-land pipelines.
- The foundation for the future `aik` CLI.
- Provider manifest discovery for dynamically loaded external providers.
- Extension points aligned with ILA (Interface-Led Architecture).

## Repository Boundary

Tools is outside AIKernel.Core. It owns operational utilities and
instrumentation Capability modules, not Core runtime behavior or provider
implementations.

Control-plane execution engines belong in AIKernel.Control. Provider
implementations belong in AIKernel.Providers or other external provider
repositories. Demo projects consume Tools, Control, and Providers; they do not
define the runtime.

AIKernel.Tools is therefore the operator and instrumentation layer of the
Semantic Runtime. It is expected to be installed beside Core, Control, and
Providers, but it should remain safe to remove from a production host that does
not need diagnostics or CLI operation. Runtime contracts continue to live in
AIKernel.NET and AIKernel.Core; physical execution continues to live in
AIKernel.Control; provider-specific drivers continue to live in
AIKernel.Providers.

Release notes:

- [English](RELEASE_NOTES.md)
- [日本語](RELEASE_NOTES-ja.md)

## Quick Start

Install the CLI tool, then run the four smallest checks:

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1

aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

These commands confirm that the CLI is installed, the Core runtime responds,
the VFS boundary can inspect the current directory, and a capability module can
be invoked through the standard `<module> <operation>` shape.

## 0.1.1 Repository Layout

Common project properties are centralized in `Directory.Build.props`.

- `src/AIKernel.CLI` - `aik` command-line entry point.
- `src/AIKernel.Tools.Capability.RomStorage` - thin bridge for
  Core-owned ROM storage contracts.
- `src/AIKernel.Tools.Instrumentation` - replay, inspector, and
  canonical formatting primitives for deterministic debugging.
- `src/AIKernel.Tools.Inspectors.KernelClock` - deterministic Kernel
  clock inspection utility.
- `src/AIKernel.Tools.Inspectors.Vfs` - VFS inspection utility.
- `src/AIKernel.Tools.Inspectors.ChatHistoryScraper` - ChatGPT
  shared-history scraper and HistoryROM exporter.

## Capability Contract Alignment

Capability modules may expose implementation-specific descriptors locally, but
the registration boundary is the shared `AIKernel.Dtos.Capabilities` contract.
The following modules provide mappers to `CapabilityModuleDescriptor`:

- `AIKernel.Core.Storage`
- `AIKernel.Core.Vfs.VfsGit`

These modules do not own Control Plane interfaces and do not mutate routing
requests directly. Control contracts are owned by `AIKernel.Abstractions.Control`
and `AIKernel.Dtos.Control`; provider-routing decisions are pure DTOs in
`AIKernel.Dtos.Routing` and are applied by Core runtime helpers.

The 0.1.1 Tools package family intentionally keeps capability ownership narrow.
Provider-oriented modules that previously lived under Tools have been moved to
AIKernel.Providers. Core-owned ROM/VFS contracts remain in AIKernel.Core. Tools
keeps only compatibility bridges and operator commands required to inspect,
invoke, or export those surfaces.

## Public Package Surface

The public package set for this repository is:

- `aik` - command-line tool package.
- `AIKernel.Tools.Instrumentation` - replay, inspection, canonical formatting,
  and deterministic serialization primitives.
- `AIKernel.Tools.Capability.RomStorage` - compatibility bridge for the
  Core-owned ROM storage contract.
- `AIKernel.Tools.Inspectors.ChatHistoryScraper` - shared-history extraction and
  deterministic Markdown / HistoryROM export.
- `AIKernel.Tools.Inspectors.KernelClock` - clock snapshot and timeline
  diagnostic command surface.
- `AIKernel.Tools.Inspectors.Vfs` - bounded local VFS diagnostic command
  surface.
- `aikernel-tools` - Python wrapper over the same public C# tooling surface.

The package set no longer contains OpenAI, CUDA, Local LLM, Dynamic Pipeline
Compiler, or VFS Git provider implementations. Those surfaces are owned by
AIKernel.Providers or AIKernel.Core and are operated through manifests or Core
standard providers.

## Documentation

- [Documentation index](docs/README.md)
- [User Guide](docs/user-guide/index.md)
- [Architecture](docs/architecture/index.md)
- [CLI operations](docs/cli/index.md)
- [Capability modules](docs/capabilities/index.md)
- [Instrumentation](docs/instrumentation/index.md)
- [Inspectors](docs/inspectors/index.md)
- [Tool pipelines](docs/pipelines/index.md)
- [Python tools wrapper](docs/python/index.md)
- [Licensing](docs/licensing/index.md)

Japanese:

- [Architecture 日本語](docs/architecture/index-ja.md)
- [CLI operations 日本語](docs/cli/index-ja.md)
- [Capability modules 日本語](docs/capabilities/index-ja.md)
- [Instrumentation 日本語](docs/instrumentation/index-ja.md)
- [Inspectors 日本語](docs/inspectors/index-ja.md)
- [Tool pipelines 日本語](docs/pipelines/index-ja.md)
- [Python tools wrapper 日本語](docs/python/index-ja.md)
- [Licensing 日本語](docs/licensing/index-ja.md)

## Build

```powershell
dotnet build AIKernel.Tools.slnx
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- --help
```

Recommended validation before publishing:

```powershell
dotnet build AIKernel.Tools.slnx -c Release --no-restore
dotnet test AIKernel.Tools.slnx -c Release --no-build
dotnet pack AIKernel.Tools.slnx -c Release --no-build -o artifacts/packages
cd python
py -m compileall src tests
py -m pytest
py -m build
```

NuGet packages are generated unsigned by default. If repository policy changes
to require signed packages, signing must be added before upload.

## CLI Smoke Commands

```powershell
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- vfs tree .
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- vfs info .
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- clock now
dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- clock timeline
```

## Operational CLI Commands

The `aik` command is organized as Linux-style subcommands:

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

Standard provider commands initialize Core standard providers before execution:
MinimalRuntimeProvider, SystemInfoProvider, VfsProvider, LocalExecutionProvider,
and SkillProvider. External provider commands continue to use provider manifest
loading under `aik providers`.

SkillProvider recursively discovers both `SKILL.md` and `Skill.MD` files. The
preferred public spelling is `SKILL.md`.

External providers are loaded from deterministic manifest files. A minimal
manifest looks like this:

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

## Package Installation

For the CLI:

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1
```

For .NET hosts:

```bash
dotnet add package AIKernel.Tools.Instrumentation --version 0.1.1
dotnet add package AIKernel.Tools.Capability.RomStorage --version 0.1.1
dotnet add package AIKernel.Tools.Inspectors.ChatHistoryScraper --version 0.1.1
dotnet add package AIKernel.Tools.Inspectors.KernelClock --version 0.1.1
dotnet add package AIKernel.Tools.Inspectors.Vfs --version 0.1.1
```

For Python hosts:

```bash
pip install aikernel-tools
```

Import the Python module as `aikernel_tools`:

```python
from aikernel_tools import CanonicalFormatter, ChatHistoryRecord
```

The wheel bundles managed AIKernel.Tools assemblies under
`aikernel_tools/native`. It is a wrapper over the public C# contract surface,
not a separate Python implementation of tooling semantics.

The Python wrapper exposes `ReplayEngine`, `ReplaySession`, `Inspector`,
`CanonicalFormatter`, `CanonicalSerializer`, `MdExporter`, `RomExporter`,
`ChatHistoryScraper`, KernelClock/VFS inspector command facades, and public
Capability contract wrappers. Python callers should treat these as managed
contract facades; they should not rely on private implementation details inside
the bundled assemblies.

## Contributor Guidelines

Tools and CLI changes must follow the shared AIKernel development discipline:

- [AIKernel Development Guidelines](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES.md)
- [AIKernel 開発ガイドライン](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES-jp.md)

CLI commands should convert failures to deterministic exit codes, avoid leaking
implementation exceptions across command boundaries, keep instrumentation pure
and reproducible, and keep Python wrappers aligned with public C# tooling
contracts.

## License

Apache License 2.0.
