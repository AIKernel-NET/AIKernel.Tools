from __future__ import annotations

from dataclasses import dataclass
from datetime import datetime, timezone
from typing import Any, Sequence

from .managed import call_static, to_string_list


@dataclass(frozen=True)
class ChatHistoryRecord:
    """[EN]
    Python wrapper for the public ChatHistoryRecord DTO.

    [JA]
    公開 ChatHistoryRecord DTO の Python wrapper です。
    """

    role: str
    content: str
    timestamp: datetime | str
    _managed: object | None = None

    def __post_init__(self):
        object.__setattr__(self, "_managed", self._managed)

    @property
    def managed(self):
        """[EN] Return the underlying C# record when materialized.

        [JA] materialize 済みの場合に背後の C# record を返します。
        """
        return self._managed

    @classmethod
    def from_managed(cls, managed) -> "ChatHistoryRecord":
        """[EN] Create a Python wrapper from a managed ChatHistoryRecord.

        [JA] managed ChatHistoryRecord から Python wrapper を作成します。
        """
        return cls(str(managed.Role), str(managed.Content), str(managed.Timestamp), managed)

    def to_managed(self):
        """[EN] Convert to AIKernel.Tools ChatHistoryRecord.

        [JA] AIKernel.Tools ChatHistoryRecord に変換します。
        """
        if self._managed is not None:
            return self._managed

        raise RuntimeError(
            "ChatHistoryRecord is materialized by the AIKernel.Tools C# formatter bridge."
        )


@dataclass(frozen=True)
class CanonicalFormatter:
    """[EN]
    Canonical formatting facade for AIKernel.Tools public exporters.

    [JA]
    AIKernel.Tools 公開 exporter 用の canonical formatting facade です。
    """

    backend: Any | None = None

    def format(self, obj: Any) -> str:
        """[EN] Format a public Tools object without changing C# semantics.

        [JA] C# semantics を変更せずに公開 Tools object を format します。
        """
        if self.backend is not None:
            return str(_call(self.backend, "format", "Format", obj))
        return _to_markdown(obj)

    def serialize(self, obj: Any) -> str:
        """[EN] Serialize a public Tools object using canonical C# exporters.

        [JA] canonical C# exporter を使って公開 Tools object を serialize します。
        """
        if self.backend is not None:
            return str(_call(self.backend, "serialize", "Serialize", obj))
        return _to_rom(obj)


CanonicalSerializer = CanonicalFormatter


class MdExporter:
    """[EN]
    Python facade for the public C# MdExporter.

    [JA]
    公開 C# MdExporter に対応する Python facade です。
    """

    @staticmethod
    def to_markdown(records: Any) -> str:
        """[EN] Format chat-history records as deterministic Markdown.

        [JA] chat-history record を deterministic Markdown として format します。
        """
        return _to_markdown(records)


class RomExporter:
    """[EN]
    Python facade for the public C# RomExporter.

    [JA]
    公開 C# RomExporter に対応する Python facade です。
    """

    @staticmethod
    def to_rom(
        records: Any,
        namespace: str = "scraper",
        name: str = "history",
        generated_at_utc: datetime | str | None = None,
    ) -> str:
        """[EN] Serialize chat-history records as deterministic HistoryROM Markdown.

        [JA] chat-history record を deterministic HistoryROM Markdown として serialize します。
        """
        if namespace == "scraper" and name == "history" and generated_at_utc is None:
            return _to_rom(records)

        generated_at = "" if generated_at_utc is None else _timestamp_text(generated_at_utc)
        return str(
            call_static(
                "AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.ChatHistoryPythonBridge",
                "AIKernel.Tools.Inspectors.ChatHistoryScraper",
                "ToRomWithMetadata",
                *_to_record_parts(records),
                namespace,
                name,
                generated_at,
            )
        )


class ChatHistoryScraper:
    """[EN]
    Python facade for the public C# ChatHistoryScraper.

    [JA]
    公開 C# ChatHistoryScraper に対応する Python facade です。
    """

    @staticmethod
    def export(url: str):
        """[EN] Export a shared ChatGPT history by delegating to C# ExportAsync.

        [JA] C# ExportAsync に委譲して共有 ChatGPT history を export します。
        """
        task = call_static(
            "AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper",
            "AIKernel.Tools.Inspectors.ChatHistoryScraper",
            "ExportAsync",
            url,
        )
        return task.GetAwaiter().GetResult()

    @staticmethod
    def to_records(history: Any) -> tuple[ChatHistoryRecord, ...]:
        """[EN] Convert a managed ChatHistory object into Python record wrappers.

        [JA] managed ChatHistory object を Python record wrapper に変換します。
        """
        records = call_static(
            "AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper",
            "AIKernel.Tools.Inspectors.ChatHistoryScraper",
            "ToRecords",
            history,
        )
        return tuple(ChatHistoryRecord.from_managed(record) for record in records)

    @staticmethod
    def to_markdown(history: Any, source_url: str) -> str:
        """[EN] Convert a managed ChatHistory object into Markdown.

        [JA] managed ChatHistory object を Markdown に変換します。
        """
        return str(
            call_static(
                "AIKernel.Tools.Inspectors.ChatHistoryScraper.ChatHistoryScraper",
                "AIKernel.Tools.Inspectors.ChatHistoryScraper",
                "ToMarkdown",
                history,
                source_url,
            )
        )


def _to_markdown(obj: Any) -> str:
    return str(
        call_static(
            "AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.ChatHistoryPythonBridge",
            "AIKernel.Tools.Inspectors.ChatHistoryScraper",
            "ToMarkdown",
            *_to_record_parts(obj),
        )
    )


def _to_rom(obj: Any) -> str:
    return str(
        call_static(
            "AIKernel.Tools.Inspectors.ChatHistoryScraper.Export.ChatHistoryPythonBridge",
            "AIKernel.Tools.Inspectors.ChatHistoryScraper",
            "ToRom",
            *_to_record_parts(obj),
        )
    )


def _to_record_parts(obj: Any):
    roles = []
    contents = []
    timestamps = []
    for item in _as_sequence(obj):
        record = _coerce_record(item)
        roles.append(record.role)
        contents.append(record.content)
        timestamps.append(_timestamp_text(record.timestamp))
    return to_string_list(roles), to_string_list(contents), to_string_list(timestamps)


def _as_sequence(obj: Any) -> Sequence[Any]:
    if isinstance(obj, (str, bytes)):
        raise TypeError("canonical chat-history formatting requires a record sequence")
    if isinstance(obj, Sequence):
        return obj
    return tuple(obj)


def _timestamp_text(value: datetime | str) -> str:
    if isinstance(value, datetime):
        if value.tzinfo is None:
            value = value.replace(tzinfo=timezone.utc)
        return value.isoformat()
    return str(value)


def _coerce_record(value: Any) -> ChatHistoryRecord:
    if isinstance(value, ChatHistoryRecord):
        return value
    role = getattr(value, "role", None) or getattr(value, "Role", None)
    content = getattr(value, "content", None) or getattr(value, "Content", None)
    timestamp = getattr(value, "timestamp", None) or getattr(value, "Timestamp", None)
    if role is None or content is None or timestamp is None:
        raise TypeError("chat-history records require role, content, and timestamp")
    return ChatHistoryRecord(str(role), str(content), str(timestamp))


def _call(target: Any, python_name: str, managed_name: str, *args) -> Any:
    method = getattr(target, python_name, None) or getattr(target, managed_name, None)
    if method is None:
        raise AttributeError(f"formatter backend must expose {python_name}()")
    return method(*args)
