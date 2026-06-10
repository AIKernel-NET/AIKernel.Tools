# AIKernel.Tools Documentation

[English](README.md)

AIKernel.Tools は AIKernel の operator / instrumentation workspace です。`aik`
CLI、replay / inspection utility、direct inspector、`aikernel-tools` Python
wrapper を含みます。

この docs は、AIOS SDK の observability / instrumentation layer として Tools を
説明します。AIOS distribution の周囲に operator command、replay、inspector、
canonical formatting、diagnostics を追加し、kernel runtime は所有しません。

公式 AIOS ディストリビューション **AIKernel.Monolith** の開発も開始されています。
Monolith は 0.1.x 系の安定化後に observability と operator tooling を
Semantic OS layer と統合する標準 reference distribution として位置づけられます。

## Sections

- [User Guide](user-guide/index-ja.md)
- [Architecture](architecture/index-ja.md)
- [CLI Operations](cli/index-ja.md)
- [Capability Modules](capabilities/index-ja.md)
- [Instrumentation](instrumentation/index-ja.md)
- [Inspectors](inspectors/index-ja.md)
- [Tool Pipelines](pipelines/index-ja.md)
- [Python Wrapper](python/index-ja.md)
- [Licensing](licensing/index-ja.md)

## どのページを読むべきか

- install から `aik` command の動作確認までを最短で確認したい場合は User Guide を
  読んでください。
- runtime、system、capabilities、providers、skills、GPU、process、logs、
  scheduler surface の command syntax を確認する場合は CLI Operations を読んでください。
- replay、VFS、clock、canonical formatting、export behavior を診断する場合は
  Instrumentation と Inspectors を読んでください。
- Python から同じ tooling surface を使う場合は Python Wrapper を読んでください。

## 最初の CLI 確認

`AIKernel.Tools.CLI` を install したら、通常の working directory で次を実行します。

```powershell
aik runtime ping
aik system info
aik system vfs --vfs-root .
aik capabilities invoke aikernel.vfs vfs.exists path=README.md
```

## Release Scope

Version 0.1.1 は AIKernel.Tools の初回公開 release line です。Provider-specific
implementation は Tools から移動済みで、Tools は operator command、
instrumentation、deterministic export、diagnostics に集中します。

## Fail-Closed CLI and Instrumentation

Tools は Core と同じ monadic safety model に従います。

- command success は `Either<L,R>` 経由で exit code へ写像する
- optional argument / optional capability descriptor は `Option<T>` を使う
- file-system inspection / exporter I/O は `Try.Run` / `Try.RunAsync` で包む
- replay / canonical formatting は hidden null や exception path に頼らず、
  deterministic state transition を明示する
