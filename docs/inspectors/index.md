# AIKernel.Tools Inspectors

[日本語](index-ja.md)

Inspectors observe AIKernel runtime-adjacent material without becoming the
runtime. They are intended for diagnostics, deterministic debugging, and
operator workflows.

## KernelClock Inspector

`AIKernel.Tools.Inspectors.KernelClock` exposes clock snapshots and timeline
events.

Commands:

```bash
aik clock now
aik clock timeline
```

The output is structured key/value text:

- `kernel_clock.utc`
- `kernel_clock.unix_ms`
- `timeline[0].event`
- `timeline[0].observed_utc`
- `timeline[0].unix_ms`

The inspector does not mutate runtime state.

## VFS Inspector

`AIKernel.Tools.Inspectors.Vfs` provides local file-system inspection helpers for
operator diagnostics.

Commands:

```bash
aik vfs tree .
aik vfs info .
```

The tree output is bounded and sorted. The info output reports path, type, size
or entry count, and last modified timestamp. This is separate from the standard
`aikernel.vfs` capability module, which is invoked through `aik capabilities`.

## ChatHistory Scraper

`AIKernel.Tools.Inspectors.ChatHistoryScraper` extracts shared ChatGPT
conversation material and exports deterministic Markdown or HistoryROM output.

The exporter preserves:

- deterministic YAML header order
- stable security tag order
- normalized line endings
- stable content hashes
- role and turn ordering

ChatHistory provider logic itself lives in AIKernel.Providers. Tools keeps only
the scraper/exporter and Python bridge required for deterministic tooling.

## Boundary

Inspectors are observation tools. They should not:

- define provider-specific implementation logic
- replace Core standard providers
- execute physical Control engines
- hide nondeterministic output behind friendly formatting

When an operation should be pipeline-callable, prefer a Capability module. When
an operation should observe state, prefer an inspector.

## Publication Boundary

Inspector packages may expose console command classes, exporter classes, and
thin Python facades. They should not introduce provider lifecycles, provider
availability probes, or capability registry logic. Those belong in Core or
Providers.

The expected public inspector package set is:

- `AIKernel.Tools.Inspectors.KernelClock`
- `AIKernel.Tools.Inspectors.Vfs`
- `AIKernel.Tools.Inspectors.ChatHistoryScraper`

Each package should include bilingual public documentation, README/icon/license
metadata through the repository-level package configuration, and smoke tests for
its command output.
