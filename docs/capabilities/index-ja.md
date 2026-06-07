# Tool Capability Modules

[English](index.md)

Tools は AIKernel pipeline のための Capability module を公開できます。初期の
capability focus は次の通りです。

- `AIKernel.Tools.Capability.ChatOpenAI` - OpenAI、Azure OpenAI、Gemini、Claude、
  互換 provider 向け external LLM Capability
  - Provides: `chat.completion`, `embedding`, `moderation`
  - Demo usage: `AIKernel.Demo.Console` ChatPipeline と
    `AIKernel.Demo.WebApi` OpenAI-compatible API
- `AIKernel.Tools.Capability.LocalLLM` - llama.cpp、Ollama、vLLM などの local LLM
  Capability
  - Provides: `chat.local`, `embedding.local`
  - Demo usage: local LLM CapabilityROM generation demos
- `AIKernel.Tools.Capability.CudaCompute` - Core から分離された CUDA Native Capability
  - Provides: `tensor.matmul`, `tensor.softmax`, `tensor.conv2d`, `tensor.layernorm`
  - Demo usage: AI-generated CUDA kernel の dynamic compile と GPU-backed fast inference
- `AIKernel.Tools.Capability.DynamicPipelineCompiler` - AI-generated DSL と LINQ monad
  pipeline を Capability として登録する dynamic compiler
  - Provides: `pipeline.compile`, `pipeline.execute`, `pipeline.validate`
  - Demo usage: `AIKernel.Demo.Pipelines` と ReplayInspector recomputation
- `AIKernel.Tools.Capability.VfsGit` - Git を VFS Provider Capability として扱う module
  - Provides: `vfs.git.read`, `vfs.git.list`, `vfs.git.checkout`
  - Demo usage: `AIKernel.Demo.Vfs.Git`
- `AIKernel.Tools.Capability.RomStorage` - VFS-backed CapabilityROM save/load/list module
  - Provides: `rom.save`, `rom.load`, `rom.list`
  - Demo usage: CapabilityROM ecosystem demos と ROM-backed recomputation

Capability module は contract definition を重複定義せず、AIKernel.NET contracts と
AIKernel.Core runtime packages を利用してください。

Inspector は観測と debugging のために分離します。既定では pipeline-callable
Capability function を定義しません。
