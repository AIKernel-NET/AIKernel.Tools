# AIKernel.Tools アーキテクチャ

[English](index.md)

AIKernel.Tools は、AIKernel の外部 utility、CLI、inspector、instrumentation
repository です。直接実行できる user-land tool、Core-owned Capability contract
向けの薄い compatibility bridge、physical execution engine に依存しない
deterministic diagnostics を含みます。

この repository は AIKernel.Core の外側にあり、operational tooling が runtime
package baseline を変更せずに進化できるようにします。

Tools は AIKernel 0.1.1 prototype validation phase に参加します。公開済みの
AIKernel.NET contracts、AIKernel.Core standard providers、external provider
manifests を、実運用に近い command-line surface から操作できることを検証します。

## Repository Role

AIKernel.Core は semantic runtime、standard providers、VFS contracts、DSL runtime、
OS-level capability contracts を所有します。AIKernel.Tools はそれらを外側から
操作します。

- `aik runtime` は MinimalRuntimeProvider の ping capability を呼び出します。
- `aik system` は SystemInfoProvider の introspection operation を呼び出します。
- `aik capabilities` は standard Capability module を list / invoke します。
- `aik exec` は LocalExecutionProvider で local DSL pipeline を実行します。
- `aik skills` は SkillProvider を通じて `SKILL.md` を読み込みます。
- `aik providers` は AIKernel.Providers または外部 provider repository の manifest
  を読み込みます。

Tools は runtime そのものではありません。operator console と deterministic
debugging layer です。

## Runtime Position

AIKernel.Tools は runtime の内部ではなく、runtime の隣に位置します。

- AIKernel.NET は shared interface、DTO、enum を定義します。
- AIKernel.Core は semantic runtime behavior、standard provider、VFS、DSL、
  `SKILL.md` parsing、provider registration contract を所有します。
- AIKernel.Control は physical execution engine と governance-oriented execution
  diagnostics を所有します。
- AIKernel.Providers は official external provider driver を所有します。
- AIKernel.Tools は operator command、deterministic observation、ROM export、
  replay-friendly formatting、Python tooling wrapper を所有します。

この分離により command-line / inspection layer は置き換え可能です。host は Tools
なしで AIKernel.Core を実行できますが、operator は Tools を install することで
Core state の inspection、standard capability の invocation、HistoryROM export、
manifest 経由の external provider loading を行えます。

## Layout

- `src/AIKernel.CLI` は `aik` などの end-user command-line surface を含みます。
- `src/AIKernel.Tools.Capability.*` は Core-owned contract 向けの薄い compatibility bridge を含みます。
- `src/AIKernel.Tools.Instrumentation` は deterministic debugging のための replay、inspection、
  canonical formatting primitive を含みます。
- `src/AIKernel.Tools.Inspectors.*` は Kernel clock、VFS、HistoryROM material を観測する diagnostic
  tools を含みます。runtime dependency にはなりません。
- `python/` は managed assembly を同梱し、public Tools surface を pythonnet 経由で
  公開する `aikernel-tools` wrapper を含みます。

Capability project は contract bridge を公開します。Control-plane execution engine は
AIKernel.Tools ではなく AIKernel.Control に属します。Provider implementation は
AIKernel.Providers または外部 provider repository に属します。

## Dependency Direction

- Tools は AIKernel.NET contracts と AIKernel.Core runtime packages に依存できます。
- Tools は external provider を manifest から動的に読み込めます。
- Tools は provider-specific implementation logic を所有しません。
- Tools は physical execution engine を所有しません。
- Tools の diagnostics は deterministic かつ replay-friendly に保ちます。

## Publication Criteria

0.1.1 package line では、Tools は次を満たすと publishable と見なします。

- provider-specific capability project が Tools package set から除外されている
- NuGet metadata に README、icon、license、project URL、repository URL、package
  tags、release notes が含まれている
- Python metadata に project URLs、license、pythonnet dependency、bundled managed
  assembly list が含まれている
- public C# class、method、property に bilingual documentation がある
- Python が C# package と同じ public instrumentation、exporter、inspector、
  contract facade を公開している
- Windows / Linux compatible assembly が native Windows-only dependency なしで
  bundle されている
- build、tests、pack、Python compile、Python tests、Python build が通る

## Related Docs

- [CLI operations](../cli/index-ja.md)
- [Capability modules](../capabilities/index-ja.md)
- [Instrumentation](../instrumentation/index-ja.md)
- [Inspectors](../inspectors/index-ja.md)
- [Tool pipelines](../pipelines/index-ja.md)
- [Python wrapper](../python/index-ja.md)
- [Licensing](../licensing/index-ja.md)
