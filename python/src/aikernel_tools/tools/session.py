from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Mapping, Sequence


@dataclass(frozen=True)
class ReplaySession:
    """[EN]
    Public replay session contract exposed to Python.

    [JA]
    Python に公開される replay session 契約です。
    """

    events: Sequence[Any] = field(default_factory=tuple)
    state: str = "Created"
    metadata: Mapping[str, str] = field(default_factory=dict)

    @classmethod
    def from_managed(cls, managed) -> "ReplaySession":
        """[EN] Create a Python session view from a managed object.

        [JA] managed object から Python session view を作成します。
        """
        events = getattr(managed, "Events", ())
        state = getattr(managed, "State", "Created")
        metadata = getattr(managed, "Metadata", {})
        return cls(tuple(events), str(state), _metadata_to_dict(metadata))


def _metadata_to_dict(metadata) -> dict[str, str]:
    keys = getattr(metadata, "Keys", None)
    if keys is not None:
        return {str(key): str(metadata[key]) for key in keys}
    return {str(key): str(value) for key, value in dict(metadata).items()}
