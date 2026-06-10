# AIKernel Tools CLI

[English](index.md)

`aik` は AIKernel.Tools の operator command です。Linux 風の subcommand 形式で
利用します。

```bash
aik <command> [arguments]
```

command surface は、standard Core providers と external provider manifests に
分かれます。

## Quick Start

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1

aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

これにより install、runtime health、VFS access、capability invocation を最小手順で
確認できます。

## Installation

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1
```

## Standard Provider Commands

これらの command は実行前に AIKernel.Core standard providers を初期化します。

- MinimalRuntimeProvider
- SystemInfoProvider
- VfsProvider
- LocalExecutionProvider
- SkillProvider

Commands:

```bash
aik runtime ping
aik system info
aik system providers
aik system capabilities
aik system vfs --vfs-root .
aik system runtime
aik capabilities list
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
aik exec run pipeline.json input.text=hello
aik skills list --root ./skills
aik skills show skill.example --root ./skills
aik skills invoke skill.example --root ./skills text=hello
```

CLI は invocation では deterministic key/value output を、system snapshot では
compact JSON を出力します。これにより shell script、ReplayLog comparison、CI
diagnostics で扱いやすくなります。

## External Provider Commands

external provider implementation は Tools に含めません。provider manifest から
install / load します。

```bash
aik install provider dynamic-pipeline
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

これにより provider implementation logic を AIKernel.Providers に保持しつつ、
operator は単一の command-line tool を利用できます。

## OS Commands

CLI は compute と process operation を Linux 風の OS surface として公開します。

```bash
aik gpu list
aik gpu run vector-add --a a.bin --b b.bin
aik run sample
aik ps
aik kill <pid-or-name>
aik restart <pid-or-name>
aik logs sample
aik schedule add --every 1m "aik system info"
```

## VFS Root

standard VFS capability を読む command は次を受け取ります。

```bash
--vfs-root <path>
```

既定値は current directory です。CLI はこの root path に read-only local file
provider を登録するため、`vfs.exists`、`vfs.read_file`、`vfs.list`、
`vfs.metadata` を外部サービスなしで実行できます。

## Skill Root

`SKILL.md` を読み込む command は次を受け取ります。

```bash
--root <path>
--skill-root <path>
```

root の既定値は `skills` または `AIKERNEL_SKILL_ROOT` environment variable です。
SkillProvider は `SKILL.md` と `Skill.MD` を再帰的に発見し、capability module
として登録します。public な主表記としては `SKILL.md` を推奨します。

## Provider Manifest Example

external provider は manifest file から discover されます。最小例は次の通りです。

```json
{
  "id": "openai.chat",
  "name": "OpenAI Chat Provider",
  "version": "0.1.1",
  "assembly": "AIKernel.Providers.OpenAI.dll",
  "capabilities": [
    "chat.completion"
  ]
}
```

## Failure Behavior

CLI command は fail-closed です。

- unknown command は non-zero exit code を返します。
- unsupported capability operation は provider error envelope を返します。
- provider manifest や `SKILL.md` root が無い場合、provider logic を黙って生成しません。
- external provider command failure は provider registry の error message として表示します。

運用上の失敗を見える形で deterministic に扱うことが目的です。

## Command Families

CLI は責務ごとに command family を分離します。

- `runtime` は minimal runtime boundary を確認します。
- `system` は安全な introspection metadata を読み取ります。
- `capabilities` は `aik capabilities invoke <module> <operation> key=value` 形式で
  Core standard capability module を操作します。
- `exec` は local DSL pipeline を LocalExecutionProvider に渡します。
- `skills` は `SKILL.md`-derived capability を discover / invoke します。
- `providers` は external provider manifest を読み込み、その capability module を
  invoke します。
- `install provider` は build 済み provider manifest と assembly を local provider
  directory に配置します。
- `clock`、`vfs`、`rom` は operator diagnostics と compatibility workflow 向けの
  direct inspector command です。

command 名は UI 風の表現ではなく、Linux-style の verb / subcommand を意図して
採用しています。

## Exit Codes

command は成功時に `0` を返します。次の場合は non-zero を返します。

- command または subcommand が不明
- 必須 argument が不足している
- provider manifest が見つからない、または読み込めない
- requested capability に対応する invoker が登録されていない
- invoked capability が error envelope を返した

failure 時も output は parseable に保ちます。CI log と replay diagnostics が失敗境界を
捕捉できるよう、error text は明示的に出力します。

## Publication Checks

CLI package 公開前に次を確認します。

- `dotnet run --project src/AIKernel.CLI/AIKernel.CLI.csproj -- --help`
- `aik runtime ping`
- `aik system info`
- `aik capabilities list`
- `AIKernel.Tools.Tests` の smoke tests
- temporary stub text を command が出力しないこと
