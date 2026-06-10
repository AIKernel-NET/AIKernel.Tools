# AIKernel.Tools Instrumentation

[English](index.md)

`AIKernel.Tools.Instrumentation` は Tools の deterministic debugging primitive を
公開します。

- `ReplayEngine`
- `ReplaySession`
- `Inspector`
- `CanonicalFormatter`
- `CanonicalSerializer`

この project は意図的に軽量です。AIKernel.Core の runtime execution や
AIKernel.Control の physical execution を所有せず、public instrumentation surface
だけを提供します。

## Replay

`ReplayEngine` は UTF-8 text file から line-oriented replay material を読み込み、
`Step()` で 1 event ずつ進め、immutable `ReplaySession` snapshot を公開します。
session には次が含まれます。

- ordered events
- replay state
- path、event count、position などの deterministic metadata

CLI smoke diagnostics と package-level deterministic debugging には十分な surface を
提供し、full runtime replay semantics は Core / Control に残します。

## Inspection

`Inspector` は compact deterministic inspection facade を提供します。

- `Inspect(value)` は type と canonical value text を返します。
- `Tree(value)` は tree-compatible canonical view を返します。
- `Diff(left, right)` は stable text diff summary を返します。

diagnostics 用であり、runtime state を変更するためのものではありません。

## Canonical Formatting

`CanonicalFormatter` と `CanonicalSerializer` は primitive value、timestamp、
dictionary、enumerable value を stable text に正規化します。dictionary key は
ordinal comparison で sort され、numeric formatting は invariant culture を使います。

これにより Windows / Linux 間で output が安定します。

## Python Alignment

Python `aikernel-tools` package は同じ public name を公開し、他の managed assembly と
一緒に `AIKernel.Tools.Instrumentation.dll` を同梱します。Python facade は明示的な
backend に委譲できますが、managed assembly は public package boundary に含まれます。

## Determinism Rules

instrumentation output は machine や run をまたいで stable であるべきです。

- dictionary key は ordinal comparison で sort する
- timestamp は invariant culture で format する
- ROM export 前に line ending を normalize する
- replay session は mutable cursor ではなく immutable snapshot を公開する
- inspection helper は value を mutate せずに説明する

新しい instrumentation primitive が deterministic text を保証できない場合は、
それを隠すのではなく nondeterministic source を示す explicit metadata を返してください。

## Package Usage

.NET では次を利用します。

```bash
dotnet add package AIKernel.Tools.Instrumentation --version 0.1.1
```

Python では次を import します。

```python
from aikernel_tools import ReplayEngine, Inspector, CanonicalFormatter
```

.NET と Python surface は alignment を保つ必要があります。public C# type を追加した
場合、Python package も同じ facade を公開するか、その型が CLI-only である理由を
明示的に document してください。
