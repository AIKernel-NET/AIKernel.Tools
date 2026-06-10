# AIKernel.Tools Instrumentation

[日本語](index-ja.md)

`AIKernel.Tools.Instrumentation` exposes the deterministic debugging primitives
for Tools:

- `ReplayEngine`
- `ReplaySession`
- `Inspector`
- `CanonicalFormatter`
- `CanonicalSerializer`

The project is intentionally lightweight. It provides the public instrumentation
surface without taking over AIKernel.Core runtime execution or AIKernel.Control
physical execution.

## Replay

`ReplayEngine` loads line-oriented replay material from a UTF-8 text file,
advances through it with `Step()`, and exposes an immutable `ReplaySession`
snapshot. The session includes:

- ordered events
- replay state
- deterministic metadata such as path, event count, and position

This is sufficient for CLI smoke diagnostics and package-level deterministic
debugging while leaving full runtime replay semantics in Core/Control.

## Inspection

`Inspector` provides a compact deterministic inspection facade:

- `Inspect(value)` returns type and canonical value text
- `Tree(value)` returns a tree-compatible canonical view
- `Diff(left, right)` returns a stable text diff summary

It is designed for diagnostics, not for mutating runtime state.

## Canonical Formatting

`CanonicalFormatter` and `CanonicalSerializer` normalize primitive values,
timestamps, dictionaries, and enumerable values into stable text. Dictionary
keys are sorted with ordinal comparison, and numeric formatting uses invariant
culture.

This keeps output stable across Windows and Linux.

## Determinism Rules

Instrumentation output should be stable across machines and runs:

- dictionary keys are sorted with ordinal comparison
- timestamps are formatted with invariant culture
- line endings are normalized before ROM export
- replay sessions expose immutable snapshots rather than mutable cursors
- inspection helpers describe values without mutating them

When a new instrumentation primitive cannot guarantee deterministic text, it
should return explicit metadata explaining the nondeterministic source instead
of hiding it.

## Python Alignment

The Python `aikernel-tools` package exposes the same public names and bundles
`AIKernel.Tools.Instrumentation.dll` with the other managed assemblies. Python
facades may delegate to explicit backends, but the managed assembly remains part
of the public package boundary.

## Package Usage

For .NET:

```bash
dotnet add package AIKernel.Tools.Instrumentation --version 0.1.1
```

For Python:

```python
from aikernel_tools import ReplayEngine, Inspector, CanonicalFormatter
```

The .NET and Python surfaces are expected to stay aligned. If a public C# type
is added, the Python package should either expose the same facade or explicitly
document why the type is CLI-only.
