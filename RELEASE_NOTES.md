# AIKernel.Tools Release Notes

[日本語](RELEASE_NOTES-ja.md)

## 0.1.0

> [EN] Tools 0.1.0 exposes the instrumentation spine: replay, inspection, and canonical formatting for deterministic debugging.
>
> [JA] Tools 0.1.0 は計測の背骨を公開──Replay・Inspector・正準フォーマットが決定論的デバッグを可能にする。

AIKernel.Tools 0.1.0 provides external Capability and inspection modules for the
prototype validation phase.

- Organize the CLI, Capability modules, and inspectors into the 0.1.0 repository
  layout.
- Provide Capability surfaces for ChatOpenAI, LocalLLM, CudaCompute,
  DynamicPipelineCompiler, VfsGit, and RomStorage.
- Stabilize CapabilityDescriptor to CapabilityModuleDescriptor mapping,
  including deterministic operation ordering, permissions, entry points,
  artifact metadata, and metadata propagation.
- Preserve deterministic ROM export for markdown, hash, YAML ordering, security
  tags, and content normalization.
- Keep Tools as an external user-land capability surface; it does not depend on
  Core or Control internals.

Tools 0.1.0 makes deterministic debugging and capability packaging visible
without turning tooling into runtime internals.
