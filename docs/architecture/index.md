# AIKernel.Tools Architecture

[日本語](index-ja.md)

AIKernel.Tools is the external utility, CLI, inspector, and instrumentation
repository for AIKernel. It contains user-land tools that can be executed
directly, thin compatibility bridges for Core-owned Capability contracts, and
deterministic diagnostics that applications can use without depending on a
physical execution engine.

The repository stays outside AIKernel.Core so operational tooling can evolve
without changing the runtime package baseline.

Tools participates in the AIKernel 0.1.1 prototype validation phase. Its role is
to prove that the published AIKernel.NET contracts, AIKernel.Core standard
providers, and external provider manifests can be operated from a practical
command-line surface.

## Repository Role

AIKernel.Core owns the semantic runtime, standard providers, VFS contracts, DSL
runtime, and OS-level capability contracts. AIKernel.Tools operates those
surfaces from the outside:

- `aik runtime` invokes the MinimalRuntimeProvider ping capability.
- `aik system` invokes SystemInfoProvider introspection operations.
- `aik capabilities` lists and invokes standard Capability modules.
- `aik exec` invokes LocalExecutionProvider for local DSL pipelines.
- `aik skills` loads `SKILL.md` files through SkillProvider.
- `aik providers` loads external provider manifests from AIKernel.Providers or
  another provider repository.

Tools does not become the runtime. It is the operator console and deterministic
debugging layer.

## Runtime Position

AIKernel.Tools sits beside the runtime rather than inside it:

- AIKernel.NET defines shared interfaces, DTOs, and enums.
- AIKernel.Core owns semantic runtime behavior, standard providers, VFS, DSL,
  `SKILL.md` parsing, and provider registration contracts.
- AIKernel.Control owns physical execution engines and governance-oriented
  execution diagnostics.
- AIKernel.Providers owns official external provider drivers.
- AIKernel.Tools owns operator commands, deterministic observation, ROM export,
  replay-friendly formatting, and Python tooling wrappers.

This separation keeps the command-line and inspection layer replaceable. A host
can run AIKernel.Core without Tools, but an operator can install Tools to inspect
Core state, invoke standard capabilities, export HistoryROM material, or load
external providers by manifest.

## Layout

- `src/AIKernel.CLI` contains end-user command-line surfaces such as `aik`.
- `src/AIKernel.Tools.Capability.*` contains thin compatibility bridges for Core-owned contracts.
- `src/AIKernel.Tools.Instrumentation` contains replay, inspection, and canonical formatting
  primitives for deterministic debugging.
- `src/AIKernel.Tools.Inspectors.*` contains diagnostic tools that observe Kernel clock, VFS, and
  HistoryROM material without becoming runtime dependencies.
- `python/` contains the `aikernel-tools` wrapper that bundles the managed
  assemblies and exposes the public Tools surface through pythonnet.

Capability projects expose contract bridges. Control-plane execution engines
belong in AIKernel.Control, not AIKernel.Tools. Provider implementations belong
in AIKernel.Providers or another external provider repository.

## Dependency Direction

- Tools may depend on AIKernel.NET contracts and AIKernel.Core runtime packages.
- Tools may load external providers dynamically by manifest.
- Tools must not own provider-specific implementation logic.
- Tools must not own physical execution engines.
- Tools must keep diagnostics deterministic and replay-friendly.

## Publication Criteria

For the 0.1.1 package line, Tools is considered publishable when:

- provider-specific capability projects are absent from the Tools package set
- NuGet metadata includes README, icon, license, project URL, repository URL,
  package tags, and release notes
- Python metadata includes project URLs, license, pythonnet dependency, and the
  bundled managed assembly list
- public C# classes, methods, and properties have bilingual documentation
- Python exposes the same public instrumentation, exporter, inspector, and
  contract facades that the C# packages expose
- Windows and Linux-compatible assemblies are bundled without native Windows-only
  dependencies
- build, tests, pack, Python compile, Python tests, and Python build pass

## Related Docs

- [CLI operations](../cli/index.md)
- [Capability modules](../capabilities/index.md)
- [Instrumentation](../instrumentation/index.md)
- [Inspectors](../inspectors/index.md)
- [Tool pipelines](../pipelines/index.md)
- [Python wrapper](../python/index.md)
- [Licensing](../licensing/index.md)
