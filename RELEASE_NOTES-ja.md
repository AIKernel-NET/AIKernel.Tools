# AIKernel.Tools リリースノート

[English](RELEASE_NOTES.md)

## 0.1.0

> [EN] Tools 0.1.0 exposes the instrumentation spine: replay, inspection, and canonical formatting for deterministic debugging.
>
> [JA] Tools 0.1.0 は計測の背骨を公開──Replay・Inspector・正準フォーマットが決定論的デバッグを可能にする。

AIKernel.Tools 0.1.0 は、prototype validation phase のための external Capability / inspection module を提供します。

- CLI、Capability module、inspector を 0.1.0 repository layout に整理します。
- ChatOpenAI、LocalLLM、CudaCompute、DynamicPipelineCompiler、VfsGit、RomStorage の Capability surface を提供します。
- CapabilityDescriptor から CapabilityModuleDescriptor への mapping を安定化します。operation ordering、permission、entry point、artifact metadata、metadata propagation は決定論的です。
- markdown、hash、YAML ordering、security tag、content normalization の deterministic ROM export を維持します。
- Tools は external user-land capability surface です。Core / Control の internal 型には依存しません。

Tools 0.1.0 は、tooling を runtime internal に変えず、決定論的デバッグと capability packaging を可視化します。
