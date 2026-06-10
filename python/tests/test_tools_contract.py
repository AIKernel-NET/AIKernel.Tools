from aikernel_tools import (
    CanonicalFormatter,
    ChatHistoryRecord,
    ChatHistoryScraper,
    InfoCommand,
    Inspector,
    MdExporter,
    NowCommand,
    ReplayEngine,
    ReplaySession,
    RomExporter,
    RomStorageCapability,
    TimelineCommand,
    TreeCommand,
    VfsGitCapability,
    tools_assemblies,
)


def test_public_import_surface():
    assert ReplayEngine
    assert ReplaySession
    assert Inspector
    assert CanonicalFormatter
    assert ChatHistoryRecord
    assert ChatHistoryScraper
    assert InfoCommand
    assert MdExporter
    assert NowCommand
    assert RomExporter
    assert TimelineCommand
    assert TreeCommand
    assert RomStorageCapability
    assert VfsGitCapability


def test_replay_engine_delegates_to_backend():
    backend = RecordingReplayBackend()
    engine = ReplayEngine(backend)

    engine.load("trace.rom")
    assert engine.session().state == "Loaded"
    assert engine.run().state == "Completed"
    assert engine.step() == "done"


def test_inspector_delegates_to_backend():
    inspector = Inspector(RecordingInspectorBackend())

    assert inspector.inspect({"b": 2, "a": 1}) == "inspect"
    assert inspector.tree({"a": 1}) == ["root", "a"]
    assert inspector.diff({"a": 1}, {"a": 2}) == {"a": (1, 2)}


def test_capability_contracts_call_managed_mappers():
    rom = RomStorageCapability("tools.rom").to_contract()
    vfs = VfsGitCapability("tools.vfs.git").to_contract()

    assert rom.required_permissions == ("rom.read", "rom.write")
    assert vfs.provided_operations == (
        "vfs.git.read",
        "vfs.git.list",
        "vfs.git.checkout",
    )


def test_canonical_formatter_uses_managed_chat_history_exporters():
    formatter = CanonicalFormatter()
    records = [
        ChatHistoryRecord("user", "hello", "2026-06-09T00:00:00+00:00"),
        ChatHistoryRecord("assistant", "world", "2026-06-09T00:00:01+00:00"),
    ]

    markdown = formatter.format(records)
    rom = formatter.serialize(records)

    assert "# Chat History" in markdown
    assert "# ROM:ChatHistory" in rom
    assert "sha256:" in rom


def test_named_chat_history_exporters_match_public_csharp_surface():
    records = [
        ChatHistoryRecord("user", "hello", "2026-06-09T00:00:00+00:00"),
        ChatHistoryRecord("assistant", "world", "2026-06-09T00:00:01+00:00"),
    ]

    markdown = MdExporter.to_markdown(records)
    rom = RomExporter.to_rom(
        records,
        namespace="python",
        name="surface",
        generated_at_utc="2026-06-09T00:00:02+00:00",
    )

    assert "# Chat History" in markdown
    assert "rom_id: 'history://python/surface'" in rom
    assert "generated_at: '2026-06-09T00:00:02" in rom


def test_tools_assembly_manifest_names():
    names = {path.name for path in tools_assemblies().assemblies}

    assert "AIKernel.Tools.Capability.RomStorage.dll" in names
    assert "AIKernel.Tools.Instrumentation.dll" in names
    assert "AIKernel.Tools.Inspectors.ChatHistoryScraper.dll" in names
    assert "AIKernel.Abstractions.dll" in names
    assert "AIKernel.Common.dll" in names
    assert "AIKernel.Core.dll" in names
    assert "AIKernel.Dtos.dll" in names
    assert "AIKernel.Enums.dll" in names
    assert "ChatHistoryProvider.dll" in names


class RecordingReplayBackend:
    def __init__(self):
        self.current = ReplaySession((), "Created", {"source": "test"})

    def load(self, path):
        self.current = ReplaySession((path,), "Loaded", {"path": path})
        return self.current

    def run(self):
        self.current = ReplaySession(self.current.events, "Completed", self.current.metadata)
        return self.current

    def step(self):
        return "done"

    def session(self):
        return self.current


class RecordingInspectorBackend:
    def inspect(self, obj):
        return "inspect"

    def tree(self, obj):
        return ["root", *obj.keys()]

    def diff(self, a, b):
        return {"a": (a["a"], b["a"])}
