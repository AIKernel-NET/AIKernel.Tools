# Tool Pipelines

[日本語](index-ja.md)

Tool pipelines should be deterministic and replay-friendly:

1. Parse input.
2. Normalize into contract DTOs.
3. Execute the tool or Capability operation.
4. Emit ROM / ReplayLog compatible output.

The `aik` CLI composes tools from `cli/`, callable modules from
`capabilities/`, and observers from `inspectors/`. User-land pipelines should
prefer Capability modules for actions and inspectors for diagnostics.
