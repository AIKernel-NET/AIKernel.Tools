# AIKernel.Tools Documentation

[日本語](README-ja.md)

AIKernel.Tools is the operator and instrumentation workspace for AIKernel. It
contains the `aik` CLI, replay/inspection utilities, direct inspectors, and the
`aikernel-tools` Python wrapper.

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
