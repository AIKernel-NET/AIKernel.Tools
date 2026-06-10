# AIKernel Tools CLI

[日本語](index-ja.md)

`aik` is the operator command for AIKernel.Tools. It uses Linux-style
subcommands:

```bash
aik <command> [arguments]
```

The command surface is split between standard Core providers and external
provider manifests.

## Quick Start

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1

aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

These checks verify installation, runtime health, VFS access, and direct
capability invocation.

## Installation

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1
```

## Standard Provider Commands

These commands initialize AIKernel.Core standard providers before execution:

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

The CLI prints deterministic key/value output for invocations and compact JSON
for system snapshots. This makes the output suitable for shell scripts,
ReplayLog comparison, and CI diagnostics.

## Command Families

The CLI separates command families by responsibility:

- `runtime` verifies the minimal runtime boundary.
- `system` reads safe introspection metadata.
- `capabilities` operates standard Core capability modules using
  `aik capabilities invoke <module> <operation> key=value`.
- `exec` sends local DSL pipelines to LocalExecutionProvider.
- `skills` discovers and invokes `SKILL.md`-derived capabilities.
- `providers` loads external provider manifests and invokes their capability
  modules.
- `gpu` lists and runs OS compute operations.
- `run`, `ps`, `kill`, and `restart` manage logical OS processes.
- `logs` reads logical process logs.
- `schedule` manages scheduled OS commands.
- `install provider` copies built provider manifests and assemblies into a local
  provider directory.
- `clock`, `vfs`, and `rom` remain direct inspector-oriented commands for
  operator diagnostics and compatibility workflows.

The command names intentionally follow Linux-style verbs and subcommands rather
than UI-oriented language.

## External Provider Commands

External provider implementations are not part of Tools. They are installed or
loaded from provider manifests:

```bash
aik install provider dynamic-pipeline
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

This keeps provider implementation logic in AIKernel.Providers while allowing
operators to use a single command-line tool.

## OS Commands

The CLI exposes a Linux-style OS surface for compute and process operation:

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

Commands that read through the standard VFS capability accept:

```bash
--vfs-root <path>
```

The default is the current directory. The CLI registers a read-only local file
provider for this root path, so commands such as `vfs.exists`, `vfs.read_file`,
`vfs.list`, and `vfs.metadata` can run without external services.

## Skill Root

Commands that load `SKILL.md` files accept:

```bash
--root <path>
--skill-root <path>
```

The root defaults to `skills` or the `AIKERNEL_SKILL_ROOT` environment variable.
The SkillProvider discovers `SKILL.md` and `Skill.MD` files recursively and
registers them as capability modules. The preferred public spelling is
`SKILL.md`.

## Provider Manifest Example

External providers are discovered from manifest files. A minimal manifest is:

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

CLI commands are fail-closed:

- unknown commands return a non-zero exit code
- unsupported capability operations return the provider error envelope
- missing provider manifests or missing `SKILL.md` roots do not silently create
  provider logic
- external provider command failures are reported with the provider registry
  error message

The goal is to make operational failures visible and deterministic.

## Exit Codes

Commands return `0` on successful operation. They return non-zero when:

- the command or subcommand is unknown
- required arguments are missing
- provider manifests cannot be found or loaded
- no invoker is registered for a requested capability
- the invoked capability returns an error envelope

Output should remain parseable even on failure. Error text is printed explicitly
so CI logs and replay diagnostics can capture the failing boundary.

## Publication Checks

Before publishing the CLI package:

- run `aik --help` through `dotnet run`
- run `aik runtime ping`
- run `aik system info`
- run `aik capabilities list`
- run smoke tests under `AIKernel.Tools.Tests`
- verify that no command prints temporary stub text
