# AIKernel.Tools Licensing

[日本語](index-ja.md)

AIKernel.Tools is licensed under the Apache License 2.0.

Tools contains command-line, diagnostic, instrumentation, and packaging code.
Apache 2.0 is used for the same practical reason as AIKernel.Core and
AIKernel.Control: the repository may contain runtime-adjacent tooling and
debugging utilities, and operators should receive explicit patent protection.

## Contract Dependencies

Shared interface and DTO packages consumed by Tools, such as
`AIKernel.Abstractions.*`, `AIKernel.Dtos.*`, and `AIKernel.Enums.*`, are part of
AIKernel.NET and are MIT licensed because they contain contracts only.

## Provider Dependencies

Provider implementations do not live in Tools. If the CLI installs or loads
external providers, those provider packages and any third-party assets remain
under their own licenses.

Tools documents command paths and manifest loading behavior; it does not
relicense external provider code, model weights, native runtime files, or remote
service terms.

## Python Wheel

The `aikernel-tools` wheel bundles managed assemblies that belong to the public
Tools package boundary and required AIKernel contract dependencies. The wheel is
a wrapper over public managed contracts and is distributed under the same
AIKernel.Tools license unless a bundled dependency states otherwise.

## Generated Artifacts

Generated NuGet packages, Python wheels, HistoryROM exports, replay snapshots,
and CLI diagnostic outputs are build or operation artifacts. They do not change
the license of source repositories, provider packages, model weights, or remote
service data referenced by the operator.

When publishing packages, remove stale artifacts for provider modules that have
moved out of Tools. Publishing an old `AIKernel.Tools.Capability.*` provider
package could incorrectly imply that Tools still owns that provider
implementation.

## Chat History Exports

`AIKernel.Tools.Inspectors.ChatHistoryScraper` can export shared conversation
material to Markdown or HistoryROM. The exporter code is Apache-2.0, but the
exported content remains subject to the origin service terms and the operator's
own rights to that conversation material. Tools normalizes and hashes exported
content; it does not relicense the content.

## Native and External Dependencies

Tools 0.1.1 does not ship Windows-only native runtime dependencies. CUDA and
other native provider implementations belong in AIKernel.Providers or native
capability repositories. If a future Tools package bundles native files, the
package must document their platform support and license terms explicitly.
