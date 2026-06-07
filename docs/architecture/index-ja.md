# AIKernel.Tools アーキテクチャ

[English](index.md)

AIKernel.Tools は、AIKernel の外部 utility / CLI repository です。直接実行できる
user-land tools と、application から参照できる Capability module を含みます。

この repository は AIKernel.Core の外側にあり、operational tooling が runtime
package baseline を変更せずに進化できるようにします。

## Layout

- `cli/` は `aik` などの end-user command-line surface を含みます。
- `capabilities/` は直接実行または user-land application から参照できる外部
  Capability module を含みます。
- `inspectors/` は Kernel clock、VFS、HistoryROM material を観測する diagnostic
  tools を含みます。runtime dependency にはなりません。

Capability project は function を公開します。Control-plane execution engine は
AIKernel.Tools ではなく AIKernel.Control に属します。
