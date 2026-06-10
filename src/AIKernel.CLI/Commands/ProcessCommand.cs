namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;
using AIKernel.Common.Results;

/// <summary>
/// [EN] Handles logical OS process commands.
/// [JA] logical OS process command を処理します。
/// </summary>
public static class ProcessCommand
{
    /// <summary>[EN] Starts a logical process. [JA] logical process を開始します。</summary>
    public static int RunProcess(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik run <name> [--wasm <path>]");
            return 1;
        }

        if (string.Equals(args[0], "haloworld", StringComparison.OrdinalIgnoreCase))
        {
            return RunHaloWorld(args);
        }

        var process = CliOsState.Run(args[0]);
        Console.WriteLine($"process: {process.Name}");
        Console.WriteLine($"pid: {process.Id}");
        Console.WriteLine($"state: {process.State}");
        return 0;
    }

    /// <summary>[EN] Lists logical processes. [JA] logical process を列挙します。</summary>
    public static int Ps(string[] args)
    {
        foreach (var process in CliOsState.ListProcesses())
        {
            Console.WriteLine($"{process.Id} {process.State} {process.Name}");
        }

        return 0;
    }

    /// <summary>[EN] Stops a logical process. [JA] logical process を停止します。</summary>
    public static int Kill(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik kill <pid-or-name>");
            return 1;
        }

        return Update(args[0], CliOsState.Kill, "killed");
    }

    /// <summary>[EN] Restarts a logical process. [JA] logical process を再起動します。</summary>
    public static int Restart(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik restart <pid-or-name>");
            return 1;
        }

        return Update(args[0], CliOsState.Restart, "restarted");
    }

    private static int Update(string idOrName, Func<string, bool> action, string verb)
    {
        if (!action(idOrName))
        {
            Console.WriteLine($"process not found: {idOrName}");
            return 1;
        }

        Console.WriteLine($"{verb}: {idOrName}");
        return 0;
    }

    private static int RunHaloWorld(string[] args)
    {
        var wasmPath = GetOption(args, "--wasm") ?? GetOption(args, "--module");
        if (string.IsNullOrWhiteSpace(wasmPath))
        {
            Console.WriteLine("Usage: aik run haloworld --wasm <path>");
            return 1;
        }

        return Try.Run(() => CliOsState.RunHaloWorld(wasmPath))
            .Match(
                error =>
                {
                    Console.WriteLine(error.Message);
                    return 1;
                },
                process =>
                {
                    Console.WriteLine($"process: {process.Name}");
                    Console.WriteLine($"pid: {process.Id}");
                    Console.WriteLine($"state: {process.State}");
                    Console.WriteLine("stdout: HaloWorld");
                    return 0;
                });
    }

    private static string? GetOption(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }
}
