# AIKernel.Tools Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.1

**June 10th, 2026 - Unifying observability.**
**2026年6月10日--可観測性を統合する。**

Unifying observability: canonical formatting, inspection, and replay surfaces
synchronize into a single instrumentation spine. 可観測性の統合--正準フォーマット・
インスペクション・リプレイ面が単一の計測スパインへ同期する。

AIKernel.Tools 0.1.1 provides instrumentation Capability and inspection modules
for the prototype validation phase.

- Organize the CLI, Capability modules, and inspectors into the 0.1.1 repository
  layout.
- Keep a compatibility bridge for Core-owned RomStorage contracts.
- Add the standard-provider CLI command surface:
  `runtime`, `system`, `capabilities`, `exec`, and `skills`.
- Add `AIKernel.Tools.Instrumentation` as the managed replay, inspector, and
  canonical formatting surface.
- Move VfsGit contracts to AIKernel.Core.Vfs.VfsGit.
- Move provider-oriented surfaces to AIKernel.Providers.
- Stabilize CapabilityDescriptor to CapabilityModuleDescriptor mapping,
  including deterministic operation ordering, permissions, entry points,
  artifact metadata, and metadata propagation.
- Preserve deterministic ROM export for markdown, hash, YAML ordering, security
  tags, and content normalization.
- Keep Tools as an external user-land instrumentation surface; provider
  implementations live outside Tools.
- Document CLI operations, instrumentation, Python wrapper scope, and licensing.

Tools 0.1.1 makes deterministic debugging and capability packaging visible
without turning tooling into runtime internals.
