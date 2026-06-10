namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;
using AIKernel.Common.Results;

/// <summary>
/// [EN] CLI command for standard system information capabilities.
/// [JA] standard system information capability 用の CLI command です。
/// </summary>
public static class SystemCommand
{
    /// <summary>
    /// [EN] Executes the system command.
    /// [JA] system command を実行します。
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
                    Console.WriteLine($"System command failed: {error.Message}");
                    return 1;
                },
                exitCode => exitCode);
    }

    private static async Task<int> RunAsync(string[] args)
    {
        var operation = args[0].ToLowerInvariant() switch
        {
            "info" => "system.info",
            "providers" => "system.providers",
            "capabilities" => "system.capabilities",
            "vfs" => "system.vfs",
            "runtime" => "system.runtime",
            var other => other
        };

        var options = StandardKernelCliOptions.Parse(args.Skip(1).ToArray(), out _);
        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);
        var result = await context.InvokeAsync(
            "aikernel.system.info",
            operation,
            new SortedDictionary<string, string>(StringComparer.Ordinal),
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["cli.command"] = $"system {args[0]}"
            }).ConfigureAwait(false);

        PrintSystemResult(result);
        return MonadicDecision.ExitCode(result.Succeeded);
    }

    private static void PrintSystemResult(
        AIKernel.Dtos.Capabilities.CapabilityInvocationResult result)
    {
        if (result.Metadata.TryGetValue("snapshot.json", out var json))
        {
            Console.WriteLine(json);
            return;
        }

        CliOutput.PrintResult(result);
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel system commands
Usage:
  aik system info
  aik system providers
  aik system capabilities
  aik system vfs [--vfs-root <path>]
  aik system runtime
""");
    }
}
