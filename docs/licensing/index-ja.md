# AIKernel.Tools Licensing

[English](index.md)

AIKernel.Tools は Apache License 2.0 で提供されます。

Tools は command-line、diagnostic、instrumentation、packaging code を含みます。
AIKernel.Core / AIKernel.Control と同じ実務的理由により Apache 2.0 を採用します。
runtime-adjacent tooling と debugging utility を含む可能性があるため、operator に
明示的な patent protection を提供します。

## Contract Dependencies

Tools が利用する共有 interface / DTO package、たとえば
`AIKernel.Abstractions.*`、`AIKernel.Dtos.*`、`AIKernel.Enums.*` は AIKernel.NET の
一部であり、contract のみを含むため MIT License です。

## Provider Dependencies

provider implementation は Tools に含めません。CLI が external provider を install /
load する場合でも、それらの provider package や third-party assets は各自の
license に従います。

Tools は command path と manifest loading behavior を文書化しますが、external
provider code、model weights、native runtime files、remote service terms を
再ライセンスしません。

## Python Wheel

`aikernel-tools` wheel は public Tools package boundary に属する managed assemblies と
必要な AIKernel contract dependencies を同梱します。wheel は public managed
contract への wrapper であり、同梱 dependency が別途指定しない限り
AIKernel.Tools と同じ license で配布されます。

## Generated Artifacts

生成された NuGet package、Python wheel、HistoryROM export、replay snapshot、CLI
diagnostic output は build / operation artifact です。operator が参照する source
repository、provider package、model weights、remote service data の license を変更しません。

package 公開時は、Tools から移管済みの provider module に対応する stale artifact を
削除してください。古い `AIKernel.Tools.Capability.*` provider package を公開すると、
Tools がまだその provider implementation を所有しているように誤解される可能性があります。

## Chat History Exports

`AIKernel.Tools.Inspectors.ChatHistoryScraper` は shared conversation material を Markdown
または HistoryROM に export できます。exporter code は Apache-2.0 ですが、exported
content は origin service terms と operator 自身がその conversation material に対して
持つ権利に従います。Tools は exported content を normalize / hash しますが、content を
再 license しません。

## Native and External Dependencies

Tools 0.1.1 は Windows-only native runtime dependency を同梱しません。CUDA などの
native provider implementation は AIKernel.Providers または native capability repository
に属します。将来 Tools package が native file を同梱する場合は、platform support と
license terms を明示的に document してください。
