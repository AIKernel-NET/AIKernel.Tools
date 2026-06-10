namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;
using AIKernel.Common.Results;

/// <summary>
/// [EN] CLI command for standard runtime provider operations.
/// [JA] standard runtime provider operation 用の CLI command です。
/// </summary>
public static class RuntimeCommand
{
    /// <summary>
    /// [EN] Executes the runtime command.
    /// [JA] runtime command を実行します。
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
                    Console.WriteLine($"Runtime command failed: {error.Message}");
                    return 1;
                },
                exitCode => exitCode);
    }

    private static async Task<int> RunAsync(string[] args)
    {
        var options = StandardKernelCliOptions.Parse(args.Skip(1).ToArray(), out _);
        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);

        return args[0].ToLowerInvariant() switch
        {
            "ping" => await PingAsync(context).ConfigureAwait(false),
            _ => Unknown(args[0])
        };
    }

    private static async Task<int> PingAsync(
        StandardKernelCliContext context)
    {
        var result = await context.InvokeAsync(
            "aikernel.runtime.ping",
            "runtime.ping",
            new SortedDictionary<string, string>(StringComparer.Ordinal),
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["cli.command"] = "runtime ping"
        }).ConfigureAwait(false);
        CliOutput.PrintResult(result);
        return MonadicDecision.ExitCode(result.Succeeded);
    }

    private static int Unknown(string command)
    {
        Console.WriteLine($"Unknown runtime command: {command}");
        ShowHelp();
        return 1;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel runtime commands
Usage:
  aik runtime ping
""");
    }
}
