# AIKernel.Tools リリースノート

[English](RELEASE_NOTES.md)

## 0.1.1

**June 10th, 2026 - Unifying observability.**
**2026年6月10日--可観測性を統合する。**

Unifying observability: canonical formatting, inspection, and replay surfaces
synchronize into a single instrumentation spine. 可観測性の統合--正準フォーマット・
インスペクション・リプレイ面が単一の計測スパインへ同期する。

AIKernel.Tools 0.1.1 は、prototype validation phase のための instrumentation Capability / inspection module を提供します。

- CLI、Capability module、inspector を 0.1.1 repository layout に整理します。
- Core-owned RomStorage contract 向け compatibility bridge を維持します。
- standard-provider CLI command surface として `runtime`、`system`、
  `capabilities`、`exec`、`skills` を追加します。
- managed replay、inspector、canonical formatting surface として
  `AIKernel.Tools.Instrumentation` を追加します。
- VfsGit contract は AIKernel.Core.Vfs.VfsGit へ移動します。
- provider-oriented surface は AIKernel.Providers へ移動します。
- CapabilityDescriptor から CapabilityModuleDescriptor への mapping を安定化します。operation ordering、permission、entry point、artifact metadata、metadata propagation は決定論的です。
- markdown、hash、YAML ordering、security tag、content normalization の deterministic ROM export を維持します。
- Tools は external user-land instrumentation surface です。provider implementation は Tools の外側に置きます。
- CLI operations、instrumentation、Python wrapper scope、licensing を文書化します。

Tools 0.1.1 は、tooling を runtime internal に変えず、決定論的デバッグと capability packaging を可視化します。
