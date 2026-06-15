# Concept Elevation Notes / 概念昇格ノート

## Canonical Reference / 正典参照

Common naming rules are maintained in AIKernel.NET:

- `AIKernel.NET/docs/canonical-language/index.md`
- `AIKernel.NET/docs/design/concept-elevation-refactoring-design.md`
- `AIKernel.NET/docs/guidelines/concept-elevation-guidelines.md`
- `AIKernel.NET/docs/migration/concept-elevation-v0.1.1.1.md`
- `AIKernel.NET/docs/todo/concept-elevation-refactoring-todo.md`

## Tools Scope / Tools の対象範囲

AIKernel.Tools owns CLI, inspectors, instrumentation, and developer-facing
observability. Concept vocabulary is allowed for upper-level viewers and
inspectors only. Low-level CLI parser, serializer, and converter names keep
technical vocabulary.

AIKernel.Tools は CLI、inspector、instrumentation、developer-facing observability
を担当します。concept vocabulary は上位 viewer / inspector のみに許可し、
低レイヤの CLI parser / serializer / converter は通常の技術語彙を維持します。

Added concept surfaces:

- `AIKernel.Tools.Instrumentation.Concepts.NomosViewer`
- `AIKernel.Tools.Instrumentation.Concepts.ChronosViewer`
- `AIKernel.Tools.Instrumentation.Concepts.PhantasiaViewer`

## Guardrails / 境界ルール

- CLI infrastructure is not renamed.
- DTO, parser, serializer, converter, and capability bridge names do not use
  philosophical prefixes.
- Viewer aliases can expose concept language without changing existing commands.
- `nomos`, `chronos`, and `replay` are additive CLI aliases; they do not rename
  `RomCommand` or `ClockCommand`.
- `RomCommand` uses `NomosViewer` for alias rendering while keeping the command
  infrastructure name unchanged.

CLI infrastructure は rename しません。viewer alias は既存 command を壊さずに
concept language を公開できます。
`nomos`、`chronos`、`replay` は追加 CLI alias であり、`RomCommand` や
`ClockCommand` を rename しません。
`RomCommand` は alias 表示に `NomosViewer` を利用しますが、command infrastructure
名は変更しません。

## Tests / テスト

`tests/AIKernel.Tools.Tests/ConceptElevationArchitectureTests.cs` guards the
naming boundary and verifies stable viewer alias values.
