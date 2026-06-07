# AIKernel.Tools Architecture

AIKernel.Tools is the external utility and CLI repository for AIKernel. It
contains user-land tools and Capability modules that can be executed directly or
referenced by applications.

The repository stays outside AIKernel.Core so operational tooling can evolve
without changing the runtime package baseline.

## Layout

- `cli/` contains end-user command-line surfaces such as `aik`.
- `capabilities/` contains external Capability modules that can be invoked
  directly or referenced by user-land applications.
- `inspectors/` contains diagnostic tools that observe Kernel clock, VFS, and
  HistoryROM material without becoming runtime dependencies.

Capability projects expose functions. Control-plane execution engines belong in
AIKernel.Control, not AIKernel.Tools.
