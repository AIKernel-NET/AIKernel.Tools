# **AIKernel.Tools**

**AIKernel の公式ツールセット兼 CLI 基盤** _Official toolset and CLI foundation for AIKernel_

##  概要 / Overview

**AIKernel.Tools** は、AIKernel の開発・運用・デバッグを支える **公式ツールセット（Tools）および CLI の基盤（Foundation）** です。

AIKernel は「AI の OS」を目指すアーキテクチャであり、 その能力は **Capability Module** として外部化され、 Provider / Observer / Operator / Semantic Runtime と連携して動作します。

このリポジトリは、AIKernel のための **標準ツールボックス**として機能し、 将来的な `aik` CLI の中核コンポーネントにもなります。

AIKernel.Tools は、2026-06-09 公開予定の 0.1.0 プロトタイプ実証フェーズに参加します。
0.0.x の設計実装フェーズで固めた契約と Core runtime を、外部 Capability module と
developer tools から検証するためのリポジトリです。

AIKernel.Tools participates in the 0.1.0 prototype validation phase scheduled
for 2026-06-09. It validates external Capability modules and developer tools
against the published AIKernel.NET contract packages and AIKernel.Core runtime.

##  目的 / Purpose

- AIKernel の能力を拡張する **Capability Modules** を提供
    
- Replay / Snapshot / Context Assembly を支えるユーティリティを提供
    
- Codex / ChatGPT / NotebookLM が利用できる外部ツールを統合
    
- AIKernel の開発者が共通で使える CLI / ライブラリを提供
    
- ILA（Interface-Led Architecture）に基づく拡張ポイントを整理
    

**AIKernel.Tools provides:**

- Capability modules that extend AIKernel’s abilities
    
- Utilities for Replay, Snapshot, and Context Assembly
    
- External tools usable by Codex, ChatGPT, and NotebookLM
    
- A shared CLI foundation for developers
    
- Extension points aligned with ILA (Interface-Led Architecture)
    

##  含まれるツール / Included Tools

### **1. ChatHistoryScraper**

ChatGPT の共有URLからチャット履歴を取得し、 **抜け漏れゼロの JSON** として保存するツール。
`.rom` 出力では、AIKernel.NET の HistoryROM 契約に合わせた hash 付き Markdown ROM を生成する。

用途:

- Codex に前回の議論を読み込ませる
    
- チャッピーの思考ログを永続化
    
- AIKernel の Replay 入力として利用
    
- NotebookLM の学習素材として利用
    

_Scrapes ChatGPT shared URLs and exports complete conversation logs as JSON._
When the output path ends with `.rom`, it emits signed AIKernel HistoryROM
Markdown that can be registered as `history://scraper/history`.

### **2. VfsInspector**

AIKernel の Virtual File System を可視化・検証するツール。

_Inspector for AIKernel’s Virtual File System._

### **3. KernelClockInspector**

IKernelClock / KernelTimeProvider の状態を可視化するツール。

_Visualizes and inspects KernelClock and KernelTimeProvider behavior._

##  Capability Modules

AIKernel.Tools に含まれるツールは、 **Capability Module** として AIKernel に統合できます。

例:

- ChatHistoryCapability
    
- VfsCapability
    
- ClockCapability
    

_Tools can be integrated into AIKernel as Capability Modules,_ _extending the system with new abilities._

### Contract alignment

Capability modules expose implementation-specific descriptors locally, but the
registration boundary is the shared `AIKernel.Dtos.Capabilities` contract.
The following modules provide contract mappers to `CapabilityModuleDescriptor`:

- `AIKernel.Tools.Capability.ChatOpenAI`
- `AIKernel.Tools.Capability.LocalLLM`
- `AIKernel.Tools.Capability.DynamicPipelineCompiler`
- `AIKernel.Tools.Capability.CudaCompute`
- `AIKernel.Tools.Capability.VfsGit`
- `AIKernel.Tools.Capability.RomStorage`

These modules do not own Control Plane interfaces and do not mutate routing
requests directly. Control contracts are owned by `AIKernel.Abstractions.Control`
and `AIKernel.Dtos.Control`; provider-routing decisions are owned as pure DTOs
in `AIKernel.Dtos.Routing` and applied by Core runtime helpers.

各 Capability module は local な実装 descriptor を持ちますが、登録境界は
`AIKernel.Dtos.Capabilities.CapabilityModuleDescriptor` に統一します。
Control Plane interface は AIKernel.NET が所有し、Tools は実装側の Capability として
接続します。Routing request の変更は Core runtime helper の責務であり、
Tools 側では provider-routing DTO を直接変更しません。

##  ドキュメント / Documentation

- `docs/architecture/index.md` — AIKernel.Tools のアーキテクチャ
    
- `docs/capabilities/index.md` — Capability Module の説明
    
- `docs/pipelines/index.md` — Tool Pipeline の作り方
    

##  開発方針 / Development Policy

- .NET 10
    
- ILA（Interface-Led Architecture）準拠
    
- Provider / Capability / Tool の三層構造
    
- 決定論的 Replay を最優先
    
- TimeProvider ではなく IKernelClock を使用

## 0.1.0 Repository Layout

Common project properties are centralized in `Directory.Build.props`.

Current layout:

- `cli/AIKernel.CLI` - `aik` command-line entry point.
- `capabilities/AIKernel.Tools.Capability.ChatOpenAI` - Chat/OpenAI Capability
  module boundary for OpenAI, Azure OpenAI, Gemini, Claude, and compatible
  external LLMs.
- `capabilities/AIKernel.Tools.Capability.LocalLLM` - local LLM Capability
  module boundary for llama.cpp, Ollama, vLLM, and similar runtimes.
- `capabilities/AIKernel.Tools.Capability.CudaCompute` - CUDA compute
  Capability module boundary for tensor operations and native GPU acceleration.
- `capabilities/AIKernel.Tools.Capability.DynamicPipelineCompiler` - semantic
  DSL and LINQ monad pipeline compiler Capability boundary.
- `capabilities/AIKernel.Tools.Capability.VfsGit` - Git-backed VFS Capability
  boundary.
- `capabilities/AIKernel.Tools.Capability.RomStorage` - ROM/HistoryROM storage
  helpers, CapabilityROM persistence, and existing ChatHistory shared models.
- `inspectors/AIKernel.Tools.Inspectors.KernelClock` - deterministic Kernel
  clock inspection utility.
- `inspectors/AIKernel.Tools.Inspectors.Vfs` - VFS inspection utility.
- `inspectors/AIKernel.Tools.Inspectors.ChatHistoryScraper` - ChatGPT
  shared-history scraper and HistoryROM exporter.

Build:

```powershell
dotnet build AIKernel.Tools.slnx
dotnet run --project cli/AIKernel.CLI/AIKernel.CLI.csproj -- --help
```
    

##  ライセンス / License

Apache License 2.0
