# AIKernel.Tools

[日本語 README](README-ja.md)

AIKernel.Tools is the official tools, CLI, inspectors, and external Capability
module workspace for AIKernel.

AIKernel.Tools participates in the AIKernel 0.1.0 prototype validation phase
scheduled for 2026-06-09. It validates that external Capability modules and
developer tools can consume the published AIKernel.NET contract packages and
AIKernel.Core runtime without owning runtime or contract definitions.

## Purpose

AIKernel.Tools provides:

- Capability modules that extend AIKernel abilities.
- Utilities for Replay, Snapshot, ROM, and Context Assembly workflows.
- External tools usable from Codex, ChatGPT, and user-land pipelines.
- The foundation for the future `aik` CLI.
- Extension points aligned with ILA (Interface-Led Architecture).

## Repository Boundary

Tools is outside AIKernel.Core. It owns operational utilities and external
Capability modules, not Core runtime behavior.

Control-plane execution engines belong in AIKernel.Control. Demo projects
consume Tools and Control; they do not define the runtime.

## 0.1.0 Repository Layout

Common project properties are centralized in `Directory.Build.props`.

- `cli/AIKernel.CLI` - `aik` command-line entry point.
- `capabilities/AIKernel.Tools.Capability.ChatOpenAI` - Chat/OpenAI Capability
  boundary for OpenAI, Azure OpenAI, Gemini, Claude, and compatible external
  LLMs.
- `capabilities/AIKernel.Tools.Capability.LocalLLM` - local LLM Capability
  boundary for llama.cpp, Ollama, vLLM, and similar runtimes.
- `capabilities/AIKernel.Tools.Capability.CudaCompute` - CUDA compute
  Capability boundary for tensor operations and native GPU acceleration.
- `capabilities/AIKernel.Tools.Capability.DynamicPipelineCompiler` - semantic
  DSL and LINQ monad pipeline compiler Capability boundary.
- `capabilities/AIKernel.Tools.Capability.VfsGit` - Git-backed VFS Capability
  boundary.
- `capabilities/AIKernel.Tools.Capability.RomStorage` - ROM / HistoryROM /
  CapabilityROM persistence helpers and ChatHistory shared models.
- `inspectors/AIKernel.Tools.Inspectors.KernelClock` - deterministic Kernel
  clock inspection utility.
- `inspectors/AIKernel.Tools.Inspectors.Vfs` - VFS inspection utility.
- `inspectors/AIKernel.Tools.Inspectors.ChatHistoryScraper` - ChatGPT
  shared-history scraper and HistoryROM exporter.

## Capability Contract Alignment

Capability modules may expose implementation-specific descriptors locally, but
the registration boundary is the shared `AIKernel.Dtos.Capabilities` contract.
The following modules provide mappers to `CapabilityModuleDescriptor`:

- `AIKernel.Tools.Capability.ChatOpenAI`
- `AIKernel.Tools.Capability.LocalLLM`
- `AIKernel.Tools.Capability.DynamicPipelineCompiler`
- `AIKernel.Tools.Capability.CudaCompute`
- `AIKernel.Tools.Capability.VfsGit`
- `AIKernel.Tools.Capability.RomStorage`

These modules do not own Control Plane interfaces and do not mutate routing
requests directly. Control contracts are owned by `AIKernel.Abstractions.Control`
and `AIKernel.Dtos.Control`; provider-routing decisions are pure DTOs in
`AIKernel.Dtos.Routing` and are applied by Core runtime helpers.

## Documentation

- [Architecture](docs/architecture/index.md)
- [Capability modules](docs/capabilities/index.md)
- [Tool pipelines](docs/pipelines/index.md)

Japanese:

- [Architecture 日本語](docs/architecture/index-ja.md)
- [Capability modules 日本語](docs/capabilities/index-ja.md)
- [Tool pipelines 日本語](docs/pipelines/index-ja.md)

## Build

```powershell
dotnet build AIKernel.Tools.slnx
dotnet run --project cli/AIKernel.CLI/AIKernel.CLI.csproj -- --help
```

## License

Apache License 2.0.
