# Tool Pipelines

[English](index.md)

Tool pipeline は決定論的で replay-friendly であるべきです。

1. 入力を parse します。
2. Contract DTO へ normalize します。
3. Tool または Capability operation を実行します。
4. ROM / ReplayLog 互換 output を emit します。

`aik` CLI は、`src/AIKernel.CLI` の tools、`src/AIKernel.Tools.Capability.*` の callable modules、
`src/AIKernel.Tools.Inspectors.*` の observers を合成します。User-land pipeline では、action には
Capability module を、diagnostics には inspector を優先して使ってください。

## Local DSL Execution

local execution は AIKernel.Core の LocalExecutionProvider が提供し、Tools から
操作します。

```bash
aik exec run pipeline.json input.text=hello
```

pipeline file は `aikernel.local.execute / pipeline.execute` に `pipeline.json`
argument として渡されます。Input value は `input.*` key として指定し、DSL runtime
へ転送されます。

最小例:

```json
{
  "type": "Pipeline",
  "steps": [
    { "type": "Step", "name": "start" }
  ]
}
```

execution metadata には DSL status、current node、executed node count、
ReplayLog hash、output values が含まれます。これにより shell automation で扱いやすく、
deterministic runtime semantics も維持できます。

## Operator Flow

Linux 風の運用では、典型的には次の流れになります。

1. `aik runtime ping` で minimal runtime boundary を確認します。
2. `aik system info` で compact system snapshot を取得します。
3. `aik capabilities list` で registered standard modules を確認します。
4. `aik exec run pipeline.json ...` で local deterministic DSL を実行します。
5. remote / native provider が必要な場合は `aik providers ...` で external provider
   manifest を読み込みます。

## Deterministic Output Contract

Tool pipeline output は CI や replay log で比較できる程度に stable であるべきです。
command は次を優先します。

- invocation envelope は key/value lines で出力する
- metadata key を sort する
- invariant culture formatting を使う
- export text の line ending を LF に normalize する
- unsupported operation には明示的な non-zero exit code を返す
- silent fallback ではなく provider error envelope を返す

これは Control-plane diagnostics と同じく、出力を interactive display だけでなく
replay にも使えるようにするためです。

## Provider Loading Flow

external provider は manifest directory から読み込みます。

```bash
aik install provider dynamic-pipeline --dir ./providers
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke dynamic.pipeline pipeline.compile --dir ./providers source=pipe.dsl
```

Tools は package name から provider behavior を推測しません。manifest が provider
identity、assembly loading、capabilities、CLI metadata、invocation metadata を記述します。
この境界により provider-specific code を Tools から外しつつ、operator には単一の shell
surface を提供します。

## Failure and Replay Guidance

pipeline tooling は fail closed であるべきです。

- pipeline input が無い場合は non-zero exit code を返す
- `SKILL.md` root が無い場合は silently ignored ではなく報告する
- provider manifest が無い場合は `aik providers` が報告する
- unsupported operation は deterministic capability result metadata を返す
- underlying capability が replay-related hash を返す場合は保持する

新しい command を追加する場合は、structured output を返すこと、temporary stub text を
出力しないことを smoke test で確認してください。
