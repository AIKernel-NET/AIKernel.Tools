namespace AIKernel.CLI.Commands;

using AIKernel.Core.Capabilities;
using AIKernel.Core.Providers;
using AIKernel.Common.Results;
using AIKernel.Dtos.Capabilities;

/// <summary>
/// [EN] CLI command for dynamically loading provider manifests and capability invokers.
/// [JA] provider manifest と capability invoker を動的に読み込む CLI command です。
/// </summary>
public static class ProviderCommand
{
    /// <summary>
    /// [EN] Executes the providers command.
    /// [JA] providers command を実行します。
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
                    Console.WriteLine($"Provider command failed: {error.Message}");
                    return 1;
                },
                exitCode => exitCode);
    }

    private static async Task<int> RunAsync(string[] args)
    {
        var subCommand = args[0].ToLowerInvariant();
        var options = ProviderCommandOptions.Parse(args.Skip(1).ToArray());
        return await (await TryLoadContextAsync(options.Directory).ConfigureAwait(false))
            .Match(
                error =>
                {
                    Console.WriteLine(error.Message);
                    return Task.FromResult(1);
                },
                context => subCommand switch
                {
                    "list" => ListProvidersAsync(context),
                    "capabilities" => ListCapabilitiesAsync(context),
                    "invoke" => InvokeAsync(args.Skip(1).ToArray(), context),
                    _ => Task.FromResult(Unknown(subCommand))
                })
            .ConfigureAwait(false);
    }

    private static Task<Result<ProviderCommandContext>> TryLoadContextAsync(string directory)
    {
        return
            from root in Try.Run(() => Path.GetFullPath(directory)).AsTask()
            from exists in ValidateDirectory(root).AsTask()
            from context in Try.RunAsync(async () =>
            {
                var capabilityRegistry = new InMemoryCapabilityModuleRegistry();
                var providerRegistry = new InMemoryProviderRegistry(capabilityRegistry);
                var manifests = Directory
                    .EnumerateFiles(root, "*.provider.json", SearchOption.AllDirectories)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();

                foreach (var manifest in manifests)
                {
                    await providerRegistry.LoadProviderFromManifest(manifest).ConfigureAwait(false);
                }

                return new ProviderCommandContext(providerRegistry, capabilityRegistry, manifests);
            })
            select context;
    }

    private static Task<int> ListProvidersAsync(ProviderCommandContext context)
    {
        foreach (var provider in context.ProviderRegistry.GetRegisteredProviders())
        {
            Console.WriteLine(provider);
        }

        if (context.Manifests.Count == 0)
        {
            Console.WriteLine("No provider manifests were found.");
        }

        return Task.FromResult(0);
    }

    private static async Task<int> ListCapabilitiesAsync(ProviderCommandContext context)
    {
        var descriptors = await context.CapabilityRegistry.ListAsync().ConfigureAwait(false);
        foreach (var descriptor in descriptors)
        {
            Console.WriteLine($"{descriptor.CapabilityId} {descriptor.Version} {descriptor.Name}");
            Console.WriteLine($"  operations: {string.Join(", ", descriptor.ProvidedOperations)}");

            foreach (var item in descriptor.Metadata
                         .Where(x => x.Key.StartsWith("cli.", StringComparison.Ordinal))
                         .OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                Console.WriteLine($"  {item.Key}: {item.Value}");
            }
        }

        if (descriptors.Count == 0)
        {
            Console.WriteLine("No provider capabilities were registered.");
        }

        return 0;
    }

    private static async Task<int> InvokeAsync(
        string[] args,
        ProviderCommandContext context)
    {
        var options = ProviderCommandOptions.Parse(args);
        var positional = options.Positional;
        if (positional.Count < 2)
        {
            Console.WriteLine("Usage: aik providers invoke <capabilityId> <operation> [key=value...] [--dir <path>]");
            return 1;
        }

        var capabilityId = positional[0];
        var operation = positional[1];
        var arguments = positional
            .Skip(2)
            .Select(SplitArgument)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

        var request = new CapabilityInvocationRequest(
            InvocationId: $"cli-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            CapabilityId: capabilityId,
            Operation: operation,
            Arguments: arguments,
            InputHash: null,
            ReplayLogHash: null,
            Metadata: new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["cli.command"] = "providers invoke"
            });

        CapabilityInvocationResult? lastResult = null;
        foreach (var invoker in context.ProviderRegistry.GetRegisteredInvokers())
        {
            var result = await invoker.InvokeAsync(request).ConfigureAwait(false);
            lastResult = result;
            if (result.Succeeded)
            {
                PrintResult(result);
                return 0;
            }
        }

        return MonadicDecision.Optional(lastResult)
            .Match(
                () =>
                {
                    Console.WriteLine("No provider invokers were registered.");
                    return 1;
                },
                result =>
                {
                    PrintResult(result);
                    return MonadicDecision.ExitCode(result.Succeeded);
                });
    }

    private static (string Key, string Value) SplitArgument(string value)
    {
        var index = value.IndexOf('=', StringComparison.Ordinal);
        return index < 0
            ? (value, "")
            : (value[..index], value[(index + 1)..]);
    }

    private static void PrintResult(CapabilityInvocationResult result)
    {
        Console.WriteLine($"invocation: {result.InvocationId}");
        Console.WriteLine($"capability: {result.CapabilityId}");
        Console.WriteLine($"succeeded: {result.Succeeded}");
        if (!string.IsNullOrWhiteSpace(result.ErrorCode))
        {
            Console.WriteLine($"error: {result.ErrorCode}");
            Console.WriteLine(result.ErrorMessage);
        }

        foreach (var item in result.Metadata.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            Console.WriteLine($"metadata.{item.Key}: {item.Value}");
        }
    }

    private static int Unknown(string command)
    {
        Console.WriteLine($"Unknown providers command: {command}");
        ShowHelp();
        return 1;
    }

    private static Result<bool> ValidateDirectory(string root)
        => Directory.Exists(root)
            ? Result<bool>.Success(true)
            : Result<bool>.Fail($"Provider directory was not found: {root}. ErrorCode=PROVIDER_DIRECTORY_NOT_FOUND");

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel provider commands
Usage:
  aik providers list [--dir ./providers]
  aik providers capabilities [--dir ./providers]
  aik providers invoke <capabilityId> <operation> [key=value...] [--dir ./providers]
""");
    }
}

internal sealed record ProviderCommandContext(
    InMemoryProviderRegistry ProviderRegistry,
    InMemoryCapabilityModuleRegistry CapabilityRegistry,
    IReadOnlyList<string> Manifests);

internal sealed record ProviderCommandOptions(
    string Directory,
    IReadOnlyList<string> Positional)
{
    /// <summary>
    /// [EN] Parses provider command options and positional arguments.
    /// [JA] provider command option と positional argument を解析します。
    /// </summary>
    public static ProviderCommandOptions Parse(string[] args)
    {
        var directory = "./providers";
        var positional = new List<string>();

        for (var index = 0; index < args.Length; index++)
        {
            var current = args[index];
            if (current is "--dir" or "-d")
            {
                if (index + 1 >= args.Length)
                {
                    throw new ArgumentException("--dir requires a path.");
                }

                directory = args[++index];
                continue;
            }

            positional.Add(current);
        }

        return new ProviderCommandOptions(directory, positional);
    }
}
