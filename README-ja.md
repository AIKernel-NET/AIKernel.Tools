# AIKernel.Tools

[English README](README.md)

AIKernel.Tools は、AIKernel の公式 tools、CLI、inspectors、外部 Capability
module のためのワークスペースです。

AIKernel.Tools は、2026-06-09 に予定している AIKernel 0.1.0 prototype
validation phase に参加します。公開済みの AIKernel.NET contract packages と
AIKernel.Core runtime を、外部 Capability module と developer tools から検証します。

## 目的

AIKernel.Tools は次を提供します。

- AIKernel の能力を拡張する Capability module
- Replay、Snapshot、ROM、Context Assembly workflow のための utility
- Codex、ChatGPT、user-land pipeline から利用できる外部 tool
- 将来の `aik` CLI の基盤
- ILA（Interface-Led Architecture）に沿った extension point

## リポジトリ境界

Tools は AIKernel.Core の外側にあります。Tools は operational utility と外部
Capability module を所有しますが、Core runtime behavior や contract definition は
所有しません。

Control-plane execution engine は AIKernel.Control に属します。Demo project は
Tools と Control を利用する側であり、runtime を定義しません。

## 0.1.0 Repository Layout

共通 project property は `Directory.Build.props` に集約されています。

- `cli/AIKernel.CLI` - `aik` command-line entry point
- `capabilities/AIKernel.Tools.Capability.ChatOpenAI` - OpenAI、Azure OpenAI、
  Gemini、Claude 互換 external LLM 向け Chat/OpenAI Capability boundary
- `capabilities/AIKernel.Tools.Capability.LocalLLM` - llama.cpp、Ollama、vLLM などの
  local LLM Capability boundary
- `capabilities/AIKernel.Tools.Capability.CudaCompute` - tensor operation と native GPU
  acceleration 向け CUDA compute Capability boundary
- `capabilities/AIKernel.Tools.Capability.DynamicPipelineCompiler` - semantic DSL と
  LINQ monad pipeline compiler Capability boundary
- `capabilities/AIKernel.Tools.Capability.VfsGit` - Git-backed VFS Capability boundary
- `capabilities/AIKernel.Tools.Capability.RomStorage` - ROM / HistoryROM /
  CapabilityROM persistence helper と ChatHistory shared model
- `inspectors/AIKernel.Tools.Inspectors.KernelClock` - deterministic Kernel clock inspector
- `inspectors/AIKernel.Tools.Inspectors.Vfs` - VFS inspector
- `inspectors/AIKernel.Tools.Inspectors.ChatHistoryScraper` - ChatGPT shared-history
  scraper と HistoryROM exporter

## Capability Contract Alignment

Capability module は local な実装 descriptor を持ってよいですが、登録境界は
`AIKernel.Dtos.Capabilities` の共有 contract に揃えます。

次の module は `CapabilityModuleDescriptor` への mapper を提供します。

- `AIKernel.Tools.Capability.ChatOpenAI`
- `AIKernel.Tools.Capability.LocalLLM`
- `AIKernel.Tools.Capability.DynamicPipelineCompiler`
- `AIKernel.Tools.Capability.CudaCompute`
- `AIKernel.Tools.Capability.VfsGit`
- `AIKernel.Tools.Capability.RomStorage`

これらの module は Control Plane interface を所有せず、routing request を直接
変更しません。Control contract は `AIKernel.Abstractions.Control` と
`AIKernel.Dtos.Control` が所有します。provider-routing decision は
`AIKernel.Dtos.Routing` の pure DTO であり、Core runtime helper が適用します。

## ドキュメント

- [Architecture](docs/architecture/index-ja.md)
- [Capability modules](docs/capabilities/index-ja.md)
- [Tool pipelines](docs/pipelines/index-ja.md)

## ビルド

```powershell
dotnet build AIKernel.Tools.slnx
dotnet run --project cli/AIKernel.CLI/AIKernel.CLI.csproj -- --help
```

## ライセンス

Apache License 2.0.
