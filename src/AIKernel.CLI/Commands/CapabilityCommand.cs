namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;
using AIKernel.Common.Results;

/// <summary>
/// [EN] CLI command for standard capability module inspection and invocation.
/// [JA] standard capability module の inspection / invocation 用 CLI command です。
/// </summary>
public static class CapabilityCommand
{
    /// <summary>
    /// [EN] Executes the capabilities command.
    /// [JA] capabilities command を実行します。
    /// </summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            ShowHelp();
            return 0;
        }

        return Try.RunAsync(() => RunAsync(args))
            .GetAwaiter()
            .GetResult()
            .Match(
                error =>
                {
                    Console.WriteLine($"Capabilities command failed: {error.Message}");
                    return 1;
                },
                exitCode => exitCode);
    }

    private static async Task<int> RunAsync(string[] args)
    {
        return args[0].ToLowerInvariant() switch
        {
            "list" => await ListAsync(args.Skip(1).ToArray()).ConfigureAwait(false),
            "invoke" => await InvokeAsync(args.Skip(1).ToArray()).ConfigureAwait(false),
            _ => Unknown(args[0])
        };
    }

    private static async Task<int> ListAsync(string[] args)
    {
        var options = StandardKernelCliOptions.Parse(args, out _);
        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);
        var descriptors = await context.CapabilityRegistry.ListAsync().ConfigureAwait(false);
        foreach (var descriptor in descriptors)
        {
            Console.WriteLine($"{NormalizeCapabilityId(descriptor.CapabilityId)} {descriptor.InvocationMode} {descriptor.Name}");
            Console.WriteLine($"  operations: {string.Join(", ", descriptor.ProvidedOperations)}");
        }

        return 0;
    }

    private static string NormalizeCapabilityId(
        string capabilityId)
        => string.Equals(capabilityId, "aikernel.vfs.read", StringComparison.Ordinal)
            ? "aikernel.vfs"
            : capabilityId;

    private static async Task<int> InvokeAsync(string[] args)
    {
        var options = StandardKernelCliOptions.Parse(args, out var positional);
        if (positional.Count < 2)
        {
            Console.WriteLine("Usage: aik capabilities invoke <module> <operation> [key=value...]");
            return 1;
        }

        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);
        var result = await context.InvokeAsync(
            positional[0],
            positional[1],
            CliOutput.ParseKeyValues(positional.Skip(2)),
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["cli.command"] = "capabilities invoke"
            }).ConfigureAwait(false);
        CliOutput.PrintResult(result);
        return MonadicDecision.ExitCode(result.Succeeded);
    }

    private static int Unknown(string command)
    {
        Console.WriteLine($"Unknown capabilities command: {command}");
        ShowHelp();
        return 1;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel capability commands
Usage:
  aik capabilities list [--root ./skills] [--vfs-root .]
  aik capabilities invoke <module> <operation> [key=value...] [--root ./skills] [--vfs-root .]
""");
    }
}
