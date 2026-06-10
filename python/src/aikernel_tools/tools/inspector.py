from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from .managed import call_static


@dataclass(frozen=True)
class Inspector:
    """[EN]
    Thin inspector facade over a public Tools backend.

    [JA]
    公開 Tools backend に委譲する薄い inspector facade です。
    """

    backend: Any | None = None

    def inspect(self, obj: Any):
        """[EN] Inspect an object through the configured backend.

        [JA] 設定された backend 経由で object を inspect します。
        """
        return _call(self.backend, "inspect", "Inspect", obj)

    def tree(self, obj: Any):
        """[EN] Return a tree view through the configured backend.

        [JA] 設定された backend 経由で tree view を返します。
        """
        return _call(self.backend, "tree", "Tree", obj)

    def diff(self, a: Any, b: Any):
        """[EN] Return a deterministic diff through the configured backend.

        [JA] 設定された backend 経由で決定論的 diff を返します。
        """
        return _call(self.backend, "diff", "Diff", a, b)


class NowCommand:
    """[EN]
    Python facade for the public C# KernelClock NowCommand.

    [JA]
    公開 C# KernelClock NowCommand に対応する Python facade です。
    """

    @staticmethod
    def run() -> None:
        """[EN] Executes the C# now command.

        [JA] C# now command を実行します。
        """
        call_static(
            "AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand",
            "AIKernel.Tools.Inspectors.KernelClock",
            "Run",
        )


class TimelineCommand:
    """[EN]
    Python facade for the public C# KernelClock TimelineCommand.

    [JA]
    公開 C# KernelClock TimelineCommand に対応する Python facade です。
    """

    @staticmethod
    def run() -> None:
        """[EN] Executes the C# timeline command.

        [JA] C# timeline command を実行します。
        """
        call_static(
            "AIKernel.Tools.Inspectors.KernelClock.Commands.TimelineCommand",
            "AIKernel.Tools.Inspectors.KernelClock",
            "Run",
        )


class InfoCommand:
    """[EN]
    Python facade for the public C# VFS InfoCommand.

    [JA]
    公開 C# VFS InfoCommand に対応する Python facade です。
    """

    @staticmethod
    def run(path: str = ".") -> None:
        """[EN] Executes the C# VFS info command.

        [JA] C# VFS info command を実行します。
        """
        call_static(
            "AIKernel.Tools.Inspectors.Vfs.Commands.InfoCommand",
            "AIKernel.Tools.Inspectors.Vfs",
            "Run",
            path,
        )


class TreeCommand:
    """[EN]
    Python facade for the public C# VFS TreeCommand.

    [JA]
    公開 C# VFS TreeCommand に対応する Python facade です。
    """

    @staticmethod
    def run(path: str = ".") -> None:
        """[EN] Executes the C# VFS tree command.

        [JA] C# VFS tree command を実行します。
        """
        call_static(
            "AIKernel.Tools.Inspectors.Vfs.Commands.TreeCommand",
            "AIKernel.Tools.Inspectors.Vfs",
            "Run",
            path,
        )


def _call(target: Any, python_name: str, managed_name: str, *args) -> Any:
    if target is None:
        raise RuntimeError(
            f"Inspector requires a backend exposing {python_name}() or {managed_name}()."
        )
    method = getattr(target, python_name, None) or getattr(target, managed_name, None)
    if method is None:
        raise AttributeError(f"inspector backend must expose {python_name}()")
    return method(*args)
