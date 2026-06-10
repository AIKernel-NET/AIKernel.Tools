namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;
using AIKernel.Common.Results;
using AIKernel.Dtos.Capabilities;

/// <summary>
/// [EN] CLI command for local DSL pipeline execution.
/// [JA] local DSL pipeline execution 用の CLI command です。
/// </summary>
public static class ExecCommand
{
    /// <summary>
    /// [EN] Executes the exec command.
    /// [JA] exec command を実行します。
    /// </summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            ShowHelp();
            return 0;
        }

        var result = Try.Run(() => RunAsync(args).GetAwaiter().GetResult());
        return ExitCode(result, "Exec command failed");
    }

    private static async Task<int> RunAsync(string[] args)
    {
        return args[0].ToLowerInvariant() switch
        {
            "run" => await RunPipelineAsync(args.Skip(1).ToArray()).ConfigureAwait(false),
            _ => Unknown(args[0])
        };
    }

    private static async Task<int> RunPipelineAsync(string[] args)
        => ExitCode(await TryRunPipelineAsync(args).ConfigureAwait(false), "Exec pipeline failed");

    private static Task<Result<int>> TryRunPipelineAsync(string[] args)
    {
        var parsed = Try.Run(() =>
        {
            var options = StandardKernelCliOptions.Parse(args, out var positional);
            return new ParsedExecOptions(options, positional);
        });

        return
            from parse in parsed.AsTask()
            from pipelinePath in ValidatePipelinePath(parse.Positional).AsTask()
            from pipelineJson in Try.Run(() => File.ReadAllText(pipelinePath)).AsTask()
            from arguments in Try.Run(() => BuildArguments(parse.Positional, pipelineJson)).AsTask()
            from context in Try.RunAsync(() => StandardKernelCliContext.CreateAsync(parse.Options))
            from invocation in Try.RunAsync(async () =>
            {
                using (context)
                {
                    return await context.InvokeAsync(
                        "aikernel.local.execute",
                        "pipeline.execute",
                        arguments,
                        new SortedDictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["cli.command"] = "exec run",
                            ["pipeline.path"] = pipelinePath
                        }).ConfigureAwait(false);
                }
            })
            select PrintInvocation(invocation);
    }

    private static Result<string> ValidatePipelinePath(IReadOnlyList<string> positional)
        => positional.Count > 0
            ? Result<string>.Success(Path.GetFullPath(positional[0]))
            : Result<string>.Fail("Usage: aik exec run <pipeline.json> [input.key=value...]");

    private static SortedDictionary<string, string> BuildArguments(IReadOnlyList<string> positional, string pipelineJson)
    {
        var arguments = new SortedDictionary<string, string>(
            CliOutput.ParseKeyValues(positional.Skip(1))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal),
            StringComparer.Ordinal)
        {
            ["pipeline.json"] = pipelineJson
        };
        return arguments;
    }

    private static int PrintInvocation(CapabilityInvocationResult result)
    {
        CliOutput.PrintResult(result);
        return MonadicDecision.ExitCode(result.Succeeded);
    }

    private static int ExitCode<T>(Result<T> result, string prefix)
        => result.Match(
            error =>
            {
                Console.WriteLine($"{prefix}: {error.Message}");
                return 1;
            },
            value => Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture));

    private sealed record ParsedExecOptions(StandardKernelCliOptions Options, IReadOnlyList<string> Positional);

    private static int Unknown(string command)
    {
        Console.WriteLine($"Unknown exec command: {command}");
        ShowHelp();
        return 1;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel local execution commands
Usage:
  aik exec run <pipeline.json> [input.name=value...] [--root ./skills] [--vfs-root .]
""");
    }
}
