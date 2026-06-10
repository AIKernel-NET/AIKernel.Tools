# User Guide

[日本語](index-ja.md)

This guide explains how to use AIKernel.Tools as an operator CLI and as a
tooling library.

Tools are the AIOS SDK observability and instrumentation layer. Use them to
operate, inspect, replay, and diagnose an AIOS distribution while keeping those
operator surfaces separate from Core and Providers.

AIKernel.Monolith is the official AIOS distribution now in development. It will
serve as the standard reference distribution that integrates CLI, replay,
inspection, and governance-oriented observability after the 0.1.x line stabilizes.

## Install the CLI

```bash
dotnet tool install -g AIKernel.Tools.CLI --version 0.1.1
```

Verify the smallest runtime surface:

```bash
aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

## Command Families

| Command | Purpose |
| --- | --- |
| `runtime` | minimal runtime health |
| `system` | safe provider/capability/VFS/runtime introspection |
| `capabilities` | invoke standard capability modules |
| `exec` | run local DSL pipelines |
| `skills` | discover and invoke `SKILL.md` capabilities |
| `providers` | load external provider manifests |
| `gpu` | run OS compute commands |
| `run`, `ps`, `kill`, `restart` | manage logical processes |
| `logs` | inspect process logs |
| `schedule` | manage scheduled commands |
| `vfs`, `clock`, `rom` | direct diagnostic inspectors |

## Invoke a Capability

The standard shape is:

```bash
aik capabilities invoke <module> <operation> key=value
```

Example:

```bash
aik capabilities invoke aikernel.vfs vfs.exists path=README.md --vfs-root .
```

## Load External Providers

```bash
aik providers list --dir ./providers
aik providers capabilities --dir ./providers
aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
```

External provider implementation remains in AIKernel.Providers. Tools only
loads manifests and invokes the declared capability boundary.

## Run a Safe Schedule

```bash
aik schedule add --every 1m "aik system info"
```

Use short intervals carefully in production-like environments.

## Use Instrumentation from .NET

```csharp
using AIKernel.Tools.Instrumentation;

var formatter = new CanonicalFormatter();
var canonical = formatter.Format(new { message = "hello" });
```

Instrumentation utilities are deterministic and intended for replay,
inspection, and test diagnostics.

## Use Python Wrapper

```python
from aikernel_tools import CanonicalFormatter, ReplayEngine

formatter = CanonicalFormatter()
print(formatter.format({"message": "hello"}))
```

The Python package delegates to the public C# tooling surface where managed
assemblies are available.

## Failure Behavior

Tools commands are operator-facing and fail closed:

- unknown command returns non-zero
- unsupported operation returns a provider/capability error envelope
- missing manifests are reported explicitly
- output is deterministic enough for scripts and CI logs

## Next Steps

- Read [CLI Operations](../cli/index.md) for full command examples.
- Read [Instrumentation](../instrumentation/index.md) for replay and canonical
  formatting.
- Read [Python Wrapper](../python/index.md) before publishing wheels.
