# Tool Pipelines

[日本語](index-ja.md)

Tool pipelines should be deterministic and replay-friendly:

1. Parse input.
2. Normalize into contract DTOs.
3. Execute the tool or Capability operation.
4. Emit ROM / ReplayLog compatible output.

The `aik` CLI composes tools from `src/AIKernel.CLI`, callable modules from
`src/AIKernel.Tools.Capability.*`, and observers from `src/AIKernel.Tools.Inspectors.*`. User-land pipelines should
prefer Capability modules for actions and inspectors for diagnostics.

## Local DSL Execution

Local execution is provided by AIKernel.Core's LocalExecutionProvider and
operated through Tools:

```bash
aik exec run pipeline.json input.text=hello
```

The pipeline file is passed as the `pipeline.json` argument to
`aikernel.local.execute / pipeline.execute`. Input values use `input.*` keys and
are forwarded to the DSL runtime.

Minimal example:

```json
{
  "type": "Pipeline",
  "steps": [
    { "type": "Step", "name": "start" }
  ]
}
```

Execution metadata includes DSL status, current node, executed node count,
ReplayLog hash, and output values. This keeps the command useful for shell
automation while preserving deterministic runtime semantics.

## Operator Flow

For Linux-style operation, a typical loop is:

1. `aik runtime ping` verifies that the minimal runtime boundary is available.
2. `aik system info` captures a compact system snapshot.
3. `aik capabilities list` confirms registered standard modules.
4. `aik exec run pipeline.json ...` executes local deterministic DSL.
5. `aik providers ...` loads external provider manifests when remote or native
   providers are required.

## Deterministic Output Contract

Tool pipeline output should be stable enough to compare in CI and replay logs.
Commands should prefer:

- key/value lines for invocation envelopes
- sorted metadata keys
- invariant culture formatting
- LF-normalized exported text
- explicit non-zero exit codes for unsupported operations
- provider error envelopes rather than silent fallback behavior

This mirrors the Control-plane expectation that diagnostics should be useful for
replay and not merely for interactive display.

## Provider Loading Flow

External providers are loaded through manifest directories:

```bash
aik install provider dynamic-pipeline --dir ./providers
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke dynamic.pipeline pipeline.compile --dir ./providers source=pipe.dsl
```

Tools does not infer provider behavior from package names. The manifest
describes provider identity, assembly loading, capabilities, CLI metadata, and
invocation metadata. This is the boundary that keeps provider-specific code out
of Tools while still giving operators a single shell surface.

## Failure and Replay Guidance

Pipeline tooling should fail closed:

- missing pipeline input returns a non-zero exit code
- missing `SKILL.md` roots are reported rather than silently ignored
- missing provider manifests are reported by `aik providers`
- unsupported operations return deterministic capability result metadata
- replay-related hashes should be preserved when the underlying capability
  returns them

When adding new commands, write smoke tests that assert the command returns
structured output and does not print temporary stub text.
