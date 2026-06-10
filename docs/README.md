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

## Sections

- [User Guide](user-guide/index.md)
- [Architecture](architecture/index.md)
- [CLI Operations](cli/index.md)
- [Capability Modules](capabilities/index.md)
- [Instrumentation](instrumentation/index.md)
- [Inspectors](inspectors/index.md)
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

Version 0.1.1 is the first public AIKernel.Tools release line. Provider-specific
implementations have moved out of Tools; Tools focuses on operator commands,
instrumentation, deterministic export, and diagnostics.

## Fail-Closed CLI and Instrumentation

Tools follows the same monadic safety model as Core:

- command success maps to exit codes through `Either<L,R>`
- optional arguments and optional capability descriptors use `Option<T>`
- file-system inspection and exporter I/O are wrapped with `Try.Run` /
  `Try.RunAsync`
- replay and canonical formatting keep deterministic state transitions explicit
  instead of relying on hidden null or exception paths
