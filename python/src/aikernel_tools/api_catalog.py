"""[EN]
Static public managed API catalog generated from the C# source tree.

[JA]
C# source tree から生成した public managed API の静的 catalog です。
"""

from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class ManagedMemberDescriptor:
    """[EN] Describes one public managed member discovered from C# source.

    [JA] C# source から検出した public managed member を表します。
    """

    kind: str
    name: str
    signature: str


@dataclass(frozen=True)
class ManagedTypeDescriptor:
    """[EN] Describes one public managed type exposed by the package.

    [JA] package が公開する public managed type を表します。
    """

    namespace: str
    name: str
    kind: str
    assembly: str
    source: str
    members: tuple[ManagedMemberDescriptor, ...] = ()

    @property
    def full_name(self) -> str:
        """[EN] Return the namespace-qualified managed type name.

        [JA] namespace で修飾された managed type 名を返します。
        """
        return f"{self.namespace}.{self.name}" if self.namespace else self.name


_CATALOG: tuple[ManagedTypeDescriptor, ...] = (
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI',
        name='Program',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Program.cs',
        members=(
            ManagedMemberDescriptor('method', 'Main', 'public static int Main(string[] args)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='CapabilityCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/CapabilityCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='ClockCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/ClockCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='ExecCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/ExecCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
            ManagedMemberDescriptor('method', 'ExitCode', 'return ExitCode(result, "Exec command failed");'),
            ManagedMemberDescriptor('method', 'ParsedExecOptions', 'return new ParsedExecOptions(options, positional);'),
            ManagedMemberDescriptor('method', 'ValidatePipelinePath', 'from pipelinePath in ValidatePipelinePath(parse.Positional).AsTask()'),
            ManagedMemberDescriptor('method', 'PrintInvocation', 'select PrintInvocation(invocation);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='GpuCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/GpuCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowUsage', 'ShowUsage();'),
            ManagedMemberDescriptor('method', 'ExitCode', 'return ExitCode(result, "GPU command failed");'),
            ManagedMemberDescriptor('method', 'ValidateVectorAddPaths', 'from paths in ValidateVectorAddPaths(options)'),
            ManagedMemberDescriptor('method', 'ValidateEqualLength', 'from _ in ValidateEqualLength(left, right)'),
            ManagedMemberDescriptor('method', 'PrintVectorAdd', 'select PrintVectorAdd(paths.OutputPath, output.Length);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='HelpCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/HelpCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'Show', 'Show();'),
            ManagedMemberDescriptor('method', 'Show', 'public static void Show()'),
            ManagedMemberDescriptor('method', 'CLI', 'AIKernel CLI (aik)'),
            ManagedMemberDescriptor('method', 'Unknown', 'public static int Unknown(string cmd)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='InstallCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/InstallCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
            ManagedMemberDescriptor('method', 'PathInfo', 'return new PathInfo(Path.GetFullPath(configured));'),
            ManagedMemberDescriptor('method', 'PathInfo', 'return new PathInfo(Path.GetFullPath(environment));'),
            ManagedMemberDescriptor('method', 'PathInfo', 'return new PathInfo(candidate);'),
            ManagedMemberDescriptor('method', 'DirectoryNotFoundException', 'throw new DirectoryNotFoundException("AIKernel.Providers source root was not found. Use --source or AIKERNEL_PROVIDER_SOURCE.");'),
            ManagedMemberDescriptor('method', 'DirectoryNotFoundException', 'throw new DirectoryNotFoundException($"Provider build output was not found for {projectRoot}. Build AIKernel.Providers first.");'),
            ManagedMemberDescriptor('method', 'Parse', 'public static InstallOptions Parse(string[] args)'),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("--dir requires a path.");'),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("--source requires a path.");'),
            ManagedMemberDescriptor('method', 'InstallOptions', 'return new InstallOptions(directory, source);'),
            ManagedMemberDescriptor('method', 'string', 'public static implicit operator string(PathInfo path) => path.Value;'),
            ManagedMemberDescriptor('method', 'ToString', 'public override string ToString() => Value;'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='LogsCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/LogsCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='ProcessCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/ProcessCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'RunProcess', 'public static int RunProcess(string[] args)'),
            ManagedMemberDescriptor('method', 'RunHaloWorld', 'return RunHaloWorld(args);'),
            ManagedMemberDescriptor('method', 'Ps', 'public static int Ps(string[] args)'),
            ManagedMemberDescriptor('method', 'Kill', 'public static int Kill(string[] args)'),
            ManagedMemberDescriptor('method', 'Update', 'return Update(args[0], CliOsState.Kill, "killed");'),
            ManagedMemberDescriptor('method', 'Restart', 'public static int Restart(string[] args)'),
            ManagedMemberDescriptor('method', 'Update', 'return Update(args[0], CliOsState.Restart, "restarted");'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='ProviderCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/ProviderCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
            ManagedMemberDescriptor('method', 'await', 'return await (await TryLoadContextAsync(options.Directory).ConfigureAwait(false))'),
            ManagedMemberDescriptor('method', 'ValidateDirectory', 'from exists in ValidateDirectory(root).AsTask()'),
            ManagedMemberDescriptor('method', 'ProviderCommandContext', 'return new ProviderCommandContext(providerRegistry, capabilityRegistry, manifests);'),
            ManagedMemberDescriptor('method', 'PrintResult', 'PrintResult(result);'),
            ManagedMemberDescriptor('method', 'Parse', 'public static ProviderCommandOptions Parse(string[] args)'),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("--dir requires a path.");'),
            ManagedMemberDescriptor('method', 'ProviderCommandOptions', 'return new ProviderCommandOptions(directory, positional);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='RomCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/RomCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'Usage', 'Usage();'),
            ManagedMemberDescriptor('method', 'View', 'return View();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='RuntimeCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/RuntimeCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='ScheduleCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/ScheduleCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowUsage', 'ShowUsage();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='SkillCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/SkillCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='SystemCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/SystemCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
            ManagedMemberDescriptor('method', 'ShowHelp', 'ShowHelp();'),
            ManagedMemberDescriptor('method', 'PrintSystemResult', 'PrintSystemResult(result);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='VersionCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/VersionCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Commands',
        name='VfsCommand',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Commands/VfsCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static int Run(string[] args)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Services',
        name='CliOsState',
        kind='class',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Services/CliOsState.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static CliProcess Run(string name)'),
            ManagedMemberDescriptor('method', 'EnsureLoaded', 'EnsureLoaded();'),
            ManagedMemberDescriptor('method', 'Save', 'Save();'),
            ManagedMemberDescriptor('method', 'RunHaloWorld', 'public static CliProcess RunHaloWorld(string wasmPath)'),
            ManagedMemberDescriptor('method', 'FileNotFoundException', 'throw new FileNotFoundException("HaloWorld.wasm was not found.", wasmPath);'),
            ManagedMemberDescriptor('method', 'ListProcesses', 'public static IReadOnlyList<CliProcess> ListProcesses()'),
            ManagedMemberDescriptor('method', 'Kill', 'public static bool Kill(string idOrName)'),
            ManagedMemberDescriptor('method', 'Restart', 'public static bool Restart(string idOrName)'),
            ManagedMemberDescriptor('method', 'Logs', 'public static IReadOnlyList<string> Logs(string process)'),
            ManagedMemberDescriptor('method', 'Schedule', 'public static CliSchedule Schedule(string every, string command)'),
            ManagedMemberDescriptor('method', 'ListSchedules', 'public static IReadOnlyList<CliSchedule> ListSchedules()'),
            ManagedMemberDescriptor('method', 'CliProcess', 'public sealed record CliProcess(string Id, string Name, string State, DateTimeOffset StartedAtUtc);'),
            ManagedMemberDescriptor('method', 'CliSchedule', 'public sealed record CliSchedule(string Id, string Every, string Command);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Services',
        name='CliProcess',
        kind='record',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Services/CliOsState.cs',
        members=(
            ManagedMemberDescriptor('method', 'CliSchedule', 'public sealed record CliSchedule(string Id, string Every, string Command);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.CLI.Services',
        name='CliSchedule',
        kind='record',
        assembly='AIKernel.CLI',
        source='AIKernel.Tools/src/AIKernel.CLI/Services/CliOsState.cs',
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Capability.RomStorage',
        name='RomStoragePythonBridge',
        kind='class',
        assembly='AIKernel.Tools.Capability.RomStorage',
        source='AIKernel.Tools/src/AIKernel.Tools.Capability.RomStorage/RomStoragePythonBridge.cs',
        members=(
            ManagedMemberDescriptor('method', 'ToContract', 'public static CapabilityModuleDescriptor ToContract('),
            ManagedMemberDescriptor('method', 'CoreRomStorageCapabilityDescriptor', 'new CoreRomStorageCapabilityDescriptor('),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.ChatHistoryScraper',
        name='ChatHistoryScraper',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.ChatHistoryScraper',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.ChatHistoryScraper/ChatHistoryScraper.cs',
        members=(
            ManagedMemberDescriptor('method', 'ExportAsync', 'public static async Task<ChatHistory> ExportAsync(string url)'),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("URL is required.", nameof(url));'),
            ManagedMemberDescriptor('method', 'InvalidOperationException', 'throw new InvalidOperationException("Conversation data not found in shared page.");'),
            ManagedMemberDescriptor('method', 'InvalidOperationException', '?? throw new InvalidOperationException("Invalid JSON table.");'),
            ManagedMemberDescriptor('method', 'ChatHistory', 'return new ChatHistory(messages);'),
            ManagedMemberDescriptor('method', 'ToRecords', 'public static IReadOnlyList<ChatHistoryRecord> ToRecords(ChatHistory history)'),
            ManagedMemberDescriptor('method', 'ToMarkdown', 'public static string ToMarkdown(ChatHistory history, string sourceUrl)'),
            ManagedMemberDescriptor('method', 'InflateRef', 'return InflateRef(table, index, cache);'),
            ManagedMemberDescriptor('method', 'Walk', 'Walk(root, messages, visited, messageKeys);'),
            ManagedMemberDescriptor('method', 'ExtractMessageIfPresent', 'ExtractMessageIfPresent(dict, messages, messageKeys);'),
            ManagedMemberDescriptor('method', 'Walk', 'Walk(v, messages, visited, messageKeys);'),
            ManagedMemberDescriptor('method', 'Walk', 'Walk(item, messages, visited, messageKeys);'),
            ManagedMemberDescriptor('method', 'InvalidOperationException', 'throw new InvalidOperationException("Chat message timestamp was not found in the shared conversation data.");'),
            ManagedMemberDescriptor('method', 'UnixSecondsToDateTimeOffset', 'return UnixSecondsToDateTimeOffset(unixLong);'),
            ManagedMemberDescriptor('method', 'UnixSecondsToDateTimeOffset', 'return UnixSecondsToDateTimeOffset(unixDouble);'),
            ManagedMemberDescriptor('method', 'FormatException', 'throw new FormatException($"Chat message timestamp is not a supported format: \'{timestamp}\'.");'),
            ManagedMemberDescriptor('method', 'ParseChatTimestamp', 'return ParseChatTimestamp(timestamp).ToString("O", CultureInfo.InvariantCulture);'),
            ManagedMemberDescriptor('method', 'FormatException', 'throw new FormatException($"Chat message timestamp is not finite: \'{unixSeconds}\'.");'),
            ManagedMemberDescriptor('method', 'HtmlScriptTag', 'HtmlScriptTag();'),
            ManagedMemberDescriptor('method', 'StreamControllerEnqueueRegex', 'StreamControllerEnqueueRegex();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.ChatHistoryScraper.Export',
        name='ChatHistoryPythonBridge',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.ChatHistoryScraper',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.ChatHistoryScraper/Export/ChatHistoryPythonBridge.cs',
        members=(
            ManagedMemberDescriptor('method', 'ToMarkdown', 'public static string ToMarkdown('),
            ManagedMemberDescriptor('method', 'ToRom', 'public static string ToRom('),
            ManagedMemberDescriptor('method', 'ToRomWithMetadata', 'public static string ToRomWithMetadata('),
            ManagedMemberDescriptor('method', 'ToRecords', 'ToRecords(roles, contents, timestamps),'),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("Chat history roles, contents, and timestamps must have the same count.");'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.ChatHistoryScraper.Export',
        name='MdExporter',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.ChatHistoryScraper',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.ChatHistoryScraper/Export/MdExporter.cs',
        members=(
            ManagedMemberDescriptor('method', 'ToMarkdown', 'public static string ToMarkdown(IReadOnlyList<ChatHistoryRecord> records)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.ChatHistoryScraper.Export',
        name='RomExporter',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.ChatHistoryScraper',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.ChatHistoryScraper/Export/RomExporter.cs',
        members=(
            ManagedMemberDescriptor('method', 'ToRom', 'public static string ToRom(IReadOnlyList<ChatHistoryRecord> records)'),
            ManagedMemberDescriptor('method', 'ToRom', 'public static string ToRom('),
            ManagedMemberDescriptor('method', 'BuildMarkdown', 'return BuildMarkdown('),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("History ROM namespace is required.", nameof(@namespace));'),
            ManagedMemberDescriptor('method', 'ArgumentException', 'throw new ArgumentException("History ROM name is required.", nameof(name));'),
            ManagedMemberDescriptor('method', 'InvalidOperationException', 'throw new InvalidOperationException("Chat history records are required.");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "# ROM:ChatHistory");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder);'),
            ManagedMemberDescriptor('method', 'InvalidOperationException', 'throw new InvalidOperationException($"Chat history record role is required. Index=\'{i}\'.");'),
            ManagedMemberDescriptor('method', 'InvalidOperationException', 'throw new InvalidOperationException($"Chat history record content is required. Index=\'{i}\'.");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, $"## Turn:{i + 1}");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, $"@role: {record.Role.Trim()}");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine('),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, NormalizeContent(record.Content));'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "---");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, $"rom_id: {QuoteYaml(romId)}");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "entity_type: \'conversation\'");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "version: \'1\'");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "source_kind: \'chat_history\'");'),
            ManagedMemberDescriptor('method', 'QuoteYaml', 'QuoteYaml(generatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "security:");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, " tags:");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, $" - {QuoteYaml(tag)}");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, "signature:");'),
            ManagedMemberDescriptor('method', 'AppendLine', 'AppendLine(builder, $" hash: {QuoteYaml(hash)}");'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.KernelClock.Commands',
        name='NowCommand',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.KernelClock',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.KernelClock/Commands/NowCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static void Run()'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.KernelClock.Commands',
        name='TimelineCommand',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.KernelClock',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.KernelClock/Commands/TimelineCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static void Run()'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.Vfs.Commands',
        name='InfoCommand',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.Vfs',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.Vfs/Commands/InfoCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static void Run()'),
            ManagedMemberDescriptor('method', 'Run', 'Run(".");'),
            ManagedMemberDescriptor('method', 'Run', 'public static void Run(string path)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Inspectors.Vfs.Commands',
        name='TreeCommand',
        kind='class',
        assembly='AIKernel.Tools.Inspectors.Vfs',
        source='AIKernel.Tools/src/AIKernel.Tools.Inspectors.Vfs/Commands/TreeCommand.cs',
        members=(
            ManagedMemberDescriptor('method', 'Run', 'public static void Run()'),
            ManagedMemberDescriptor('method', 'Run', 'Run(".");'),
            ManagedMemberDescriptor('method', 'Run', 'public static void Run(string path)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation',
        name='CanonicalFormatter',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/CanonicalFormatter.cs',
        members=(
            ManagedMemberDescriptor('method', 'Format', 'public string Format(object? value)'),
            ManagedMemberDescriptor('method', 'TryFormat', 'public Result<string> TryFormat(object? value)'),
            ManagedMemberDescriptor('method', 'Serialize', 'public string Serialize(object? value)'),
            ManagedMemberDescriptor('method', 'TrySerialize', 'public Result<string> TrySerialize(object? value)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation',
        name='CanonicalSerializer',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/CanonicalSerializer.cs',
        members=(
            ManagedMemberDescriptor('method', 'Serialize', 'public static string Serialize(object? value)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation',
        name='Inspector',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Inspector.cs',
        members=(
            ManagedMemberDescriptor('method', 'Inspect', 'public string Inspect(object? value)'),
            ManagedMemberDescriptor('method', 'TryInspect', 'public Result<string> TryInspect(object? value)'),
            ManagedMemberDescriptor('method', 'Tree', 'public string Tree(object? value)'),
            ManagedMemberDescriptor('method', 'TryTree', 'public Result<string> TryTree(object? value)'),
            ManagedMemberDescriptor('method', 'Diff', 'public string Diff(object? left, object? right)'),
            ManagedMemberDescriptor('method', 'TryDiff', 'public Result<string> TryDiff(object? left, object? right)'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation',
        name='ReplayEngine',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/ReplayEngine.cs',
        members=(
            ManagedMemberDescriptor('method', 'Load', 'public ReplayEngine Load(string path)'),
            ManagedMemberDescriptor('method', 'TryLoad', 'public Result<IReplayEngine> TryLoad(string path)'),
            ManagedMemberDescriptor('method', 'ValidatePath', 'from validPath in ValidatePath(path)'),
            ManagedMemberDescriptor('method', 'Run', 'public ReplaySession Run()'),
            ManagedMemberDescriptor('method', 'TryRun', 'public Result<ReplaySession> TryRun()'),
            ManagedMemberDescriptor('method', 'TrySession', 'return TrySession();'),
            ManagedMemberDescriptor('method', 'Step', 'public string? Step()'),
            ManagedMemberDescriptor('method', 'TryStep', 'public Result<Option<string>> TryStep()'),
            ManagedMemberDescriptor('method', 'Session', 'public ReplaySession Session()'),
            ManagedMemberDescriptor('method', 'TrySession', 'public Result<ReplaySession> TrySession()'),
            ManagedMemberDescriptor('method', 'ReplaySession', 'return new ReplaySession(_events, _state, metadata);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation',
        name='ReplaySession',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/ReplaySession.cs',
        members=(
            ManagedMemberDescriptor('property', 'Events', 'public IReadOnlyList<string> Events { get; }'),
            ManagedMemberDescriptor('property', 'State', 'public string State { get; }'),
            ManagedMemberDescriptor('property', 'Metadata', 'public IReadOnlyDictionary<string, string> Metadata { get; }'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation.Abstractions',
        name='ICanonicalFormatter',
        kind='interface',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Abstractions/ICanonicalFormatter.cs',
        members=(
            ManagedMemberDescriptor('method', 'TryFormat', 'Result<string> TryFormat(object? value);'),
            ManagedMemberDescriptor('method', 'TrySerialize', 'Result<string> TrySerialize(object? value);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation.Abstractions',
        name='IInspector',
        kind='interface',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Abstractions/IInspector.cs',
        members=(
            ManagedMemberDescriptor('method', 'TryInspect', 'Result<string> TryInspect(object? value);'),
            ManagedMemberDescriptor('method', 'TryTree', 'Result<string> TryTree(object? value);'),
            ManagedMemberDescriptor('method', 'TryDiff', 'Result<string> TryDiff(object? left, object? right);'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation.Abstractions',
        name='IReplayEngine',
        kind='interface',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Abstractions/IReplayEngine.cs',
        members=(
            ManagedMemberDescriptor('method', 'TryLoad', 'Result<IReplayEngine> TryLoad(string path);'),
            ManagedMemberDescriptor('method', 'TryRun', 'Result<ReplaySession> TryRun();'),
            ManagedMemberDescriptor('method', 'TryStep', 'Result<Option<string>> TryStep();'),
            ManagedMemberDescriptor('method', 'TrySession', 'Result<ReplaySession> TrySession();'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation.Concepts',
        name='ChronosViewer',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Concepts/ConceptElevationViewers.cs',
        members=(
            ManagedMemberDescriptor('method', 'Alias', 'public string Alias(string target = "replay")'),
            ManagedMemberDescriptor('method', 'Alias', 'public string Alias(string target = "scene")'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation.Concepts',
        name='NomosViewer',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Concepts/ConceptElevationViewers.cs',
        members=(
            ManagedMemberDescriptor('method', 'Alias', 'public string Alias(string target = "rom")'),
            ManagedMemberDescriptor('method', 'Alias', 'public string Alias(string target = "replay")'),
            ManagedMemberDescriptor('method', 'Alias', 'public string Alias(string target = "scene")'),
        ),
    ),
    ManagedTypeDescriptor(
        namespace='AIKernel.Tools.Instrumentation.Concepts',
        name='PhantasiaViewer',
        kind='class',
        assembly='AIKernel.Tools.Instrumentation',
        source='AIKernel.Tools/src/AIKernel.Tools.Instrumentation/Concepts/ConceptElevationViewers.cs',
        members=(
            ManagedMemberDescriptor('method', 'Alias', 'public string Alias(string target = "scene")'),
        ),
    ),
)


def managed_api_catalog() -> tuple[ManagedTypeDescriptor, ...]:
    """[EN] Return the generated public managed API catalog.

    [JA] 生成済み public managed API catalog を返します。
    """
    return _CATALOG


def managed_type_names() -> tuple[str, ...]:
    """[EN] Return all namespace-qualified managed type names.

    [JA] namespace 修飾済み managed type 名をすべて返します。
    """
    return tuple(item.full_name for item in _CATALOG)


def find_managed_type(full_name: str) -> ManagedTypeDescriptor | None:
    """[EN] Find a managed type descriptor by namespace-qualified name.

    [JA] namespace 修飾名から managed type descriptor を検索します。
    """
    for item in _CATALOG:
        if item.full_name == full_name:
            return item
    return None


def managed_api_summary() -> dict[str, int]:
    """[EN] Return public managed API counts by assembly.

    [JA] assembly ごとの public managed API 件数を返します。
    """
    summary: dict[str, int] = {}
    for item in _CATALOG:
        summary[item.assembly] = summary.get(item.assembly, 0) + 1
    return dict(sorted(summary.items()))


__all__ = [
    "ManagedMemberDescriptor",
    "ManagedTypeDescriptor",
    "find_managed_type",
    "managed_api_catalog",
    "managed_api_summary",
    "managed_type_names",
]
