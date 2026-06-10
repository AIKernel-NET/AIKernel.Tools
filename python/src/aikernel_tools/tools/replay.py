from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from .session import ReplaySession


@dataclass
class ReplayEngine:
    """[EN]
    Thin replay facade that delegates to a public Tools backend.

    [JA]
    公開 Tools backend に委譲する薄い replay facade です。
    """

    backend: Any | None = None
    _session: ReplaySession = ReplaySession()

    def load(self, path: str) -> "ReplayEngine":
        """[EN] Load replay material through the configured backend.

        [JA] 設定された backend 経由で replay material を読み込みます。
        """
        result = _call(self.backend, "load", "Load", path)
        if result is not None:
            self._session = _coerce_session(result)
        return self

    def run(self):
        """[EN] Run replay through the configured backend.

        [JA] 設定された backend 経由で replay を実行します。
        """
        result = _call(self.backend, "run", "Run")
        if result is not None:
            self._session = _coerce_session(result)
        return result

    def step(self):
        """[EN] Advance replay by one deterministic step.

        [JA] replay を決定論的に 1 step 進めます。
        """
        result = _call(self.backend, "step", "Step")
        if result is not None and not isinstance(result, (str, int, float, bool)):
            self._session = _coerce_session(result)
        return result

    def session(self) -> ReplaySession:
        """[EN] Return the current replay session view.

        [JA] 現在の replay session view を返します。
        """
        result = _call(self.backend, "session", "Session", required=False)
        if result is not None:
            self._session = _coerce_session(result)
        return self._session


def _call(target: Any, python_name: str, managed_name: str, *args, required: bool = True) -> Any:
    if target is None:
        if required:
            raise RuntimeError(
                f"ReplayEngine requires a backend exposing {python_name}() or {managed_name}()."
            )
        return None
    method = getattr(target, python_name, None) or getattr(target, managed_name, None)
    if method is None:
        if required:
            raise AttributeError(f"replay backend must expose {python_name}()")
        return None
    return method(*args)


def _coerce_session(value: Any) -> ReplaySession:
    if isinstance(value, ReplaySession):
        return value
    return ReplaySession.from_managed(value)
