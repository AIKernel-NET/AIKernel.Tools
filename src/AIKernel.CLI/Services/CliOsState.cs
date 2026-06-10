namespace AIKernel.CLI.Services;

using AIKernel.Common.Results;

/// <summary>
/// [EN] In-memory OS state used by CLI smoke commands.
/// [JA] CLI smoke command が使用する in-memory OS state です。
/// </summary>
public static class CliOsState
{
    private static readonly List<CliProcess> Processes = [];
    private static readonly List<CliSchedule> Schedules = [];
    private static readonly List<string> LogEntries = [];
    private static bool loaded;

    /// <summary>[EN] Starts a logical process. [JA] logical process を開始します。</summary>
    public static CliProcess Run(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureLoaded();
        var process = new CliProcess(Guid.NewGuid().ToString("N"), name, "Running", DateTimeOffset.UtcNow);
        Processes.Add(process);
        LogEntries.Add($"{process.Name}: ProcessStarted {process.Id}");
        Save();
        return process;
    }

    /// <summary>[EN] Runs the deterministic HaloWorld WASM smoke process. [JA] 決定論的な HaloWorld WASM smoke process を実行します。</summary>
    public static CliProcess RunHaloWorld(string wasmPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wasmPath);
        if (!File.Exists(wasmPath))
        {
            throw new FileNotFoundException("HaloWorld.wasm was not found.", wasmPath);
        }

        EnsureLoaded();
        var process = new CliProcess(Guid.NewGuid().ToString("N"), "haloworld", "Stopped", DateTimeOffset.UtcNow);
        Processes.Add(process);
        LogEntries.Add($"{process.Name}: ProcessStarted {process.Id}");
        LogEntries.Add($"{process.Name}: Stdout HaloWorld");
        LogEntries.Add($"{process.Name}: ProcessStopped {process.Id}");
        Save();
        return process;
    }

    /// <summary>[EN] Lists logical processes. [JA] logical process を列挙します。</summary>
    public static IReadOnlyList<CliProcess> ListProcesses()
    {
        EnsureLoaded();
        return Processes.ToArray();
    }

    /// <summary>[EN] Marks a logical process as stopped. [JA] logical process を stopped として mark します。</summary>
    public static bool Kill(string idOrName)
        => UpdateProcess(idOrName, "Stopped", "ProcessStopped");

    /// <summary>[EN] Marks a logical process as running. [JA] logical process を running として mark します。</summary>
    public static bool Restart(string idOrName)
        => UpdateProcess(idOrName, "Running", "ProcessStarted");

    /// <summary>[EN] Returns log lines for a process name. [JA] process 名に対応する log 行を返します。</summary>
    public static IReadOnlyList<string> Logs(string process)
    {
        EnsureLoaded();
        return LogEntries
            .Where(entry => entry.StartsWith(process + ":", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>[EN] Adds a scheduled command. [JA] scheduled command を追加します。</summary>
    public static CliSchedule Schedule(string every, string command)
    {
        EnsureLoaded();
        var schedule = new CliSchedule(Guid.NewGuid().ToString("N"), every, command);
        Schedules.Add(schedule);
        LogEntries.Add($"scheduler: ScheduleAdded {schedule.Id} {schedule.Every} {schedule.Command}");
        Save();
        return schedule;
    }

    /// <summary>[EN] Lists scheduled commands. [JA] scheduled command を列挙します。</summary>
    public static IReadOnlyList<CliSchedule> ListSchedules()
    {
        EnsureLoaded();
        return Schedules.ToArray();
    }

    private static bool UpdateProcess(string idOrName, string state, string eventName)
    {
        EnsureLoaded();
        var index = Processes.FindIndex(process =>
            string.Equals(process.Id, idOrName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(process.Name, idOrName, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return false;
        }

        var current = Processes[index];
        var updated = current with { State = state };
        Processes[index] = updated;
        LogEntries.Add($"{updated.Name}: {eventName} {updated.Id}");
        Save();
        return true;
    }

    private static void EnsureLoaded()
    {
        if (loaded)
        {
            return;
        }

        loaded = true;
        var path = StatePath();
        if (!File.Exists(path))
        {
            return;
        }

        Try
            .Run(() => System.Text.Json.JsonSerializer.Deserialize<CliStateFile>(File.ReadAllText(path)))
            .Bind(state => MonadicDecision.Optional(state)
                .Match(
                    () => Result<bool>.Success(false),
                    value =>
                    {
                        Processes.Clear();
                        Processes.AddRange(value.Processes);
                        Schedules.Clear();
                        Schedules.AddRange(value.Schedules);
                        LogEntries.Clear();
                        LogEntries.AddRange(value.LogEntries);
                        return Result<bool>.Success(true);
                    }))
            .Match(
                _ =>
                {
                    Processes.Clear();
                    Schedules.Clear();
                    LogEntries.Clear();
                    return false;
                },
                loadedState => loadedState);
    }

    private static void Save()
    {
        var path = StatePath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var state = new CliStateFile(Processes.ToArray(), Schedules.ToArray(), LogEntries.ToArray());
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(state));
    }

    private static string StatePath()
    {
        var stateDirectory = Environment.GetEnvironmentVariable("AIKERNEL_CLI_STATE_DIR");
        if (string.IsNullOrWhiteSpace(stateDirectory))
        {
            stateDirectory = Path.Combine(Environment.CurrentDirectory, ".aik");
        }

        return Path.Combine(stateDirectory, "cli-state.json");
    }

    private sealed record CliStateFile(CliProcess[] Processes, CliSchedule[] Schedules, string[] LogEntries);
}

/// <summary>
/// [EN] CLI logical process snapshot.
/// [JA] CLI logical process snapshot です。
/// </summary>
public sealed record CliProcess(string Id, string Name, string State, DateTimeOffset StartedAtUtc);

/// <summary>
/// [EN] CLI scheduled command snapshot.
/// [JA] CLI scheduled command snapshot です。
/// </summary>
public sealed record CliSchedule(string Id, string Every, string Command);
