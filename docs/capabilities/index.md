# Tool Capability Modules

Tools may expose Capability modules for AIKernel pipelines. The initial
capability focus is:

- `AIKernel.Tools.Capability.ChatOpenAI` - external LLM Capability for OpenAI,
  Azure OpenAI, Gemini, Claude, and compatible providers.
  - Provides: `chat.completion`, `embedding`, `moderation`.
  - Demo usage: `AIKernel.Demo.Console` ChatPipeline and
    `AIKernel.Demo.WebApi` OpenAI-compatible API.
- `AIKernel.Tools.Capability.LocalLLM` - local LLM Capability for llama.cpp,
  Ollama, vLLM, and similar runtimes.
  - Provides: `chat.local`, `embedding.local`.
  - Demo usage: local LLM CapabilityROM generation demos.
- `AIKernel.Tools.Capability.CudaCompute` - CUDA Native Capability separated
  from Core and exposed as callable tensor functions.
  - Provides: `tensor.matmul`, `tensor.softmax`, `tensor.conv2d`,
    `tensor.layernorm`.
  - Demo usage: dynamically compiled AI-generated CUDA kernels and GPU-backed
    fast inference demos.
- `AIKernel.Tools.Capability.DynamicPipelineCompiler` - dynamic compiler for
  AI-generated DSL and LINQ monad pipelines registered as Capabilities.
  - Provides: `pipeline.compile`, `pipeline.execute`, `pipeline.validate`.
  - Demo usage: `AIKernel.Demo.Pipelines` and ReplayInspector recomputation.
- `AIKernel.Tools.Capability.VfsGit` - Git as a VFS Provider Capability.
  - Provides: `vfs.git.read`, `vfs.git.list`, `vfs.git.checkout`.
  - Demo usage: `AIKernel.Demo.Vfs.Git`.
- `AIKernel.Tools.Capability.RomStorage` - CapabilityROM save/load/list module
  backed by VFS.
  - Provides: `rom.save`, `rom.load`, `rom.list`.
  - Demo usage: CapabilityROM ecosystem demos and ROM-backed recomputation.

Capability modules should use AIKernel.NET contracts and AIKernel.Core runtime
packages rather than duplicating contract definitions.

Inspectors remain separate because they observe and debug; they do not define
pipeline-callable Capability functions by default.
