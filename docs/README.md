# AIKernel.Tools Documentation

[日本語](README-ja.md)

AIKernel.Tools is the operator and instrumentation workspace for AIKernel. It
contains the `aik` CLI, replay/inspection utilities, direct inspectors, and the
`aikernel-tools` Python wrapper.

These docs describe Tools as the AIOS SDK observability and instrumentation
layer. Tools adds operator commands, replay, inspectors, canonical formatting,
and diagnostics around an AIOS distribution without owning the kernel runtime.

AIKernel.Monolith is the official AIOS distribution now in development. It is
planned as the standard reference distribution that integrates observability
and operator tooling with the Semantic OS layers after the 0.1.x line stabilizes.

## Cross-Repository Alignment

Shared repository boundaries, v0.1.3 GPU integration versioning, dependency order,
PyPI Trusted Publishing, and Python wrapper scope are defined by
[AIKernel GPU rev3 Migration v0.1.3](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/migration/v0.1.3-gpu-rev3-migration.md).
The historical v0.1.1.1 validation rules remain available in
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1.md).
When a change crosses repositories, start with the
[Cross-Repository Developer Guide v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1.md).

Tools owns CLI, inspection, replay, instrumentation, canonical formatting, and
diagnostics. It must not own kernel runtime behavior, provider execution, or
scenario runtime implementation.

## Sections

- [User Guide](user-guide/index.md)
- [Architecture](architecture/index.md)
- [CLI Operations](cli/index.md)
- [Capability Modules](capabilities/index.md)
- [Instrumentation](instrumentation/index.md)
- [Inspectors](inspectors/index.md)
- [Concept Elevation Notes / 概念昇格ノート](development/concept-elevation.md)
- [Tool Pipelines](pipelines/index.md)
- [Python Wrapper](python/index.md)
- [Licensing](licensing/index.md)

## Which Page Should I Read?

- Read User Guide when you want the shortest path from installation to a
  working `aik` command.
- Read CLI Operations when you need command syntax for runtime, system,
  capabilities, providers, skills, GPU, process, logs, and scheduler surfaces.
- Read Instrumentation and Inspectors when you are diagnosing replay, VFS,
  clock, canonical formatting, or export behavior.
- Read Python Wrapper when you need the same tooling surface from Python.

## First CLI Checks

After installing `AIKernel.Tools.CLI`, run these commands from a normal working
directory:

```powershell
aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

## Release Scope

Version 0.1.3 is the unified public AIKernel.Tools release line. Provider-specific
implementations have moved out of Tools; Tools focuses on operator commands,
instrumentation, deterministic export, and diagnostics.

Version 0.1.3 is the current canonical integration line. Use
`0.1.3-dev{build-number}` for local NuGet package references and
`0.1.3.dev{build-number}` for local `aikernel-tools` wheel validation. Stable
package artifacts are created later in dependency order.

## Fail-Closed CLI and Instrumentation

Tools follows the same monadic safety model as Core:

- command success maps to exit codes through `Either<L,R>`
- optional arguments and optional capability descriptors use `Option<T>`
- file-system inspection and exporter I/O are wrapped with `Try.Run` /
  `Try.RunAsync`
- replay and canonical formatting keep deterministic state transitions explicit
  instead of relying on hidden null or exception paths
