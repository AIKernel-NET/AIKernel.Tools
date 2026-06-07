# Tool Pipelines

[English](index.md)

Tool pipeline は決定論的で replay-friendly であるべきです。

1. 入力を parse します。
2. Contract DTO へ normalize します。
3. Tool または Capability operation を実行します。
4. ROM / ReplayLog 互換 output を emit します。

`aik` CLI は、`cli/` の tools、`capabilities/` の callable modules、
`inspectors/` の observers を合成します。User-land pipeline では、action には
Capability module を、diagnostics には inspector を優先して使ってください。
