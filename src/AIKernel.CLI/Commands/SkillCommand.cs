namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;
using AIKernel.Common.Results;
using AIKernel.Dtos.Capabilities;

/// <summary>
/// [EN] CLI command for SKILL.md-backed standard capabilities.
/// [JA] SKILL.md backed standard capability 用の CLI command です。
/// </summary>
public static class SkillCommand
{
    /// <summary>
    /// [EN] Executes the skills command.
    /// [JA] skills command を実行します。
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
                    Console.WriteLine($"Skills command failed: {error.Message}");
                    return 1;
                },
                exitCode => exitCode);
    }

    private static async Task<int> RunAsync(string[] args)
    {
        return args[0].ToLowerInvariant() switch
        {
            "list" => await ListAsync(args.Skip(1).ToArray()).ConfigureAwait(false),
            "show" => await ShowAsync(args.Skip(1).ToArray()).ConfigureAwait(false),
            "invoke" => await InvokeAsync(args.Skip(1).ToArray()).ConfigureAwait(false),
            _ => Unknown(args[0])
        };
    }

    private static async Task<int> ListAsync(string[] args)
    {
        var options = StandardKernelCliOptions.Parse(args, out _);
        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);
        var skills = (await context.CapabilityRegistry.ListAsync().ConfigureAwait(false))
            .Where(x => x.Metadata.TryGetValue("provider", out var provider) &&
                        string.Equals(provider, "aikernel.skill", StringComparison.Ordinal))
            .OrderBy(x => x.CapabilityId, StringComparer.Ordinal)
            .ToArray();

        foreach (var skill in skills)
        {
            Console.WriteLine($"{skill.CapabilityId} {skill.Name}");
            if (skill.Metadata.TryGetValue("skill.source_path", out var source))
            {
                Console.WriteLine($"  source: {source}");
            }
        }

        if (skills.Length == 0)
        {
            Console.WriteLine("No SKILL.md capabilities were found.");
        }

        return 0;
    }

    private static async Task<int> ShowAsync(string[] args)
    {
        var options = StandardKernelCliOptions.Parse(args, out var positional);
        if (positional.Count == 0)
        {
            Console.WriteLine("Usage: aik skills show <capabilityId> [--root ./skills]");
            return 1;
        }

        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);
        return MonadicDecision.Optional(await context.CapabilityRegistry.ResolveAsync(positional[0]).ConfigureAwait(false))
            .Match(
                () =>
                {
                    Console.WriteLine($"Skill capability was not found: {positional[0]}");
                    return 1;
                },
                descriptor =>
                {
                    Console.WriteLine($"{descriptor.CapabilityId} {descriptor.Name}");
                    Console.WriteLine($"version: {descriptor.Version}");
                    Console.WriteLine($"operations: {string.Join(", ", descriptor.ProvidedOperations)}");
                    foreach (var item in descriptor.Metadata.OrderBy(x => x.Key, StringComparer.Ordinal))
                    {
                        Console.WriteLine($"{item.Key}: {item.Value}");
                    }

                    return 0;
                });
    }

    private static async Task<int> InvokeAsync(string[] args)
    {
        var options = StandardKernelCliOptions.Parse(args, out var positional);
        if (positional.Count == 0)
        {
            Console.WriteLine("Usage: aik skills invoke <capabilityId> [operation] [key=value...] [--root ./skills]");
            return 1;
        }

        var capabilityId = positional[0];
        var operation = OperationArgument(positional, capabilityId);
        var argumentStart = ArgumentStart(operation, capabilityId);

        using var context = await StandardKernelCliContext.CreateAsync(options).ConfigureAwait(false);
        var result = await context.InvokeAsync(
            capabilityId,
            operation,
            CliOutput.ParseKeyValues(positional.Skip(argumentStart)),
            new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["cli.command"] = "skills invoke"
        }).ConfigureAwait(false);
        CliOutput.PrintResult(result);
        return MonadicDecision.ExitCode(result.Succeeded);
    }

    private static string OperationArgument(IReadOnlyList<string> positional, string capabilityId)
        => OperationArgumentOption(positional).Match(() => capabilityId, value => value);

    private static Option<string> OperationArgumentOption(IReadOnlyList<string> positional)
        => positional.Count > 1 && !positional[1].Contains('=', StringComparison.Ordinal)
            ? Option<string>.Some(positional[1])
            : Option<string>.None();

    private static int ArgumentStart(string operation, string capabilityId)
        => OperationSelection(operation, capabilityId).Match(_ => 1, _ => 2);

    private static Either<string, string> OperationSelection(string operation, string capabilityId)
        => string.Equals(operation, capabilityId, StringComparison.Ordinal)
            ? Either<string, string>.FromLeft("default-operation")
            : Either<string, string>.FromRight("explicit-operation");

    private static int Unknown(string command)
    {
        Console.WriteLine($"Unknown skills command: {command}");
        ShowHelp();
        return 1;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel skill commands
Usage:
  aik skills list [--root ./skills]
  aik skills show <capabilityId> [--root ./skills]
  aik skills invoke <capabilityId> [operation] [key=value...] [--root ./skills]
""");
    }
}
