# AIKernel.Tools Documentation

[English](README.md)

AIKernel.Tools は AIKernel の operator / instrumentation workspace です。`aik`
CLI、replay / inspection utility、direct inspector、`aikernel-tools` Python
wrapper を含みます。

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
