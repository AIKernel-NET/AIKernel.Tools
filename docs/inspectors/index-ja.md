# AIKernel.Tools Inspectors

[English](index.md)

Inspector は AIKernel runtime-adjacent material を観測しますが、runtime そのものには
なりません。diagnostics、deterministic debugging、operator workflow のためのものです。

## KernelClock Inspector

`AIKernel.Tools.Inspectors.KernelClock` は clock snapshot と timeline event を公開します。

Commands:

```bash
aik clock now
aik clock timeline
```

output は structured key/value text です。

- `kernel_clock.utc`
- `kernel_clock.unix_ms`
- `timeline[0].event`
- `timeline[0].observed_utc`
- `timeline[0].unix_ms`

この inspector は runtime state を変更しません。

## VFS Inspector

`AIKernel.Tools.Inspectors.Vfs` は operator diagnostics 用の local file-system
inspection helper を提供します。

Commands:

```bash
aik vfs tree .
aik vfs info .
```

tree output は bounded かつ sorted です。info output は path、type、size または
entry count、last modified timestamp を報告します。これは `aik capabilities`
から呼び出す standard `aikernel.vfs` capability module とは別です。

## ChatHistory Scraper

`AIKernel.Tools.Inspectors.ChatHistoryScraper` は shared ChatGPT conversation material
を抽出し、deterministic Markdown または HistoryROM output として export します。

exporter は次を維持します。

- deterministic YAML header order
- stable security tag order
- normalized line endings
- stable content hashes
- role and turn ordering

ChatHistory provider logic 自体は AIKernel.Providers にあります。Tools は
deterministic tooling に必要な scraper / exporter / Python bridge だけを保持します。

## Boundary

Inspector は observation tool です。次の責務は持ちません。

- provider-specific implementation logic を定義する
- Core standard providers を置き換える
- physical Control engine を実行する
- nondeterministic output を friendly formatting で隠す

pipeline-callable にすべき operation では Capability module を優先し、state を
観測する operation では inspector を優先します。

## Publication Boundary

inspector package は console command class、exporter class、thin Python facade を
公開できます。ただし provider lifecycle、provider availability probe、capability
registry logic は導入しません。それらは Core または Providers に属します。

expected public inspector package set は次の通りです。

- `AIKernel.Tools.Inspectors.KernelClock`
- `AIKernel.Tools.Inspectors.Vfs`
- `AIKernel.Tools.Inspectors.ChatHistoryScraper`

各 package は bilingual public documentation、repository-level package configuration
経由の README / icon / license metadata、command output の smoke tests を含む必要が
あります。
