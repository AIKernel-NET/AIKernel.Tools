namespace AIKernel.CLI.Services;

using AIKernel.Abstractions.Capabilities;
using AIKernel.Abstractions.Providers;
using AIKernel.Core.Vfs.Local;
using AIKernel.Dtos.Capabilities;
using AIKernel.Hosting;
using AIKernel.Vfs;
using Microsoft.Extensions.DependencyInjection;

internal sealed class StandardKernelCliContext : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private StandardKernelCliContext(
        ServiceProvider serviceProvider,
        ICapabilityModuleRegistry capabilityRegistry,
        IReadOnlyList<ICapabilityModuleInvoker> invokers,
        IReadOnlyList<IProvider> providers)
    {
        _serviceProvider = serviceProvider;
        CapabilityRegistry = capabilityRegistry;
        Invokers = invokers;
        Providers = providers;
    }

    /// <summary>
    /// [EN] Gets the standard capability registry used by CLI commands.
    /// [JA] CLI command が使用する standard capability registry を取得します。
    /// </summary>
    public ICapabilityModuleRegistry CapabilityRegistry { get; }

    /// <summary>
    /// [EN] Gets the standard capability invokers available to the CLI.
    /// [JA] CLI で利用可能な standard capability invoker を取得します。
    /// </summary>
    public IReadOnlyList<ICapabilityModuleInvoker> Invokers { get; }

    /// <summary>
    /// [EN] Gets the standard providers initialized for the CLI session.
    /// [JA] CLI session 用に初期化された standard provider を取得します。
    /// </summary>
    public IReadOnlyList<IProvider> Providers { get; }

    /// <summary>
    /// [EN] Creates and initializes a standard AIKernel CLI context.
    /// [JA] standard AIKernel CLI context を作成して初期化します。
    /// </summary>
    public static async Task<StandardKernelCliContext> CreateAsync(
        StandardKernelCliOptions options,
        CancellationToken cancellationToken = default)
    {
        var services = new ServiceCollection();
        if (!string.IsNullOrWhiteSpace(options.SkillRoot))
        {
            Environment.SetEnvironmentVariable("AIKERNEL_SKILL_ROOT", options.SkillRoot);
        }

        services.AddSingleton<IVfsProvider>(_ => new LocalFileProvider(
            new LocalFileProviderOptions(
                options.VfsRoot,
                allowWrite: false,
                providerId: "cli-local-file",
                name: "CLI Local File Provider")));

        services.AddAIKernelCore();

        var provider = services.BuildServiceProvider();
        var providers = provider.GetServices<IProvider>().ToArray();
        foreach (var standardProvider in providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await standardProvider.InitializeAsync().ConfigureAwait(false);
        }

        return new StandardKernelCliContext(
            provider,
            provider.GetRequiredService<ICapabilityModuleRegistry>(),
            provider.GetServices<ICapabilityModuleInvoker>().ToArray(),
            providers);
    }

    /// <summary>
    /// [EN] Invokes a registered standard capability and returns its deterministic result.
    /// [JA] 登録済み standard capability を呼び出し、deterministic result を返します。
    /// </summary>
    public async Task<CapabilityInvocationResult> InvokeAsync(
        string capabilityId,
        string operation,
        IReadOnlyDictionary<string, string> arguments,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var request = new CapabilityInvocationRequest(
            InvocationId: $"cli-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            CapabilityId: capabilityId,
            Operation: operation,
            Arguments: arguments,
            InputHash: null,
            ReplayLogHash: null,
            Metadata: metadata ?? new SortedDictionary<string, string>(StringComparer.Ordinal));

        CapabilityInvocationResult? last = null;
        foreach (var candidateCapabilityId in ExpandCapabilityAliases(capabilityId))
        {
            var candidateRequest = candidateCapabilityId == request.CapabilityId
                ? request
                : request with { CapabilityId = candidateCapabilityId };
            foreach (var invoker in Invokers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await invoker.InvokeAsync(candidateRequest, cancellationToken).ConfigureAwait(false);
                last = result;
                if (result.Succeeded)
                {
                    var resultMetadata = new SortedDictionary<string, string>(StringComparer.Ordinal);
                    foreach (var pair in result.Metadata)
                    {
                        resultMetadata[pair.Key] = pair.Value;
                    }

                    resultMetadata["compat.capability_id"] = candidateCapabilityId;
                    return candidateCapabilityId == capabilityId
                        ? result
                        : result with
                        {
                            CapabilityId = capabilityId,
                            Metadata = resultMetadata
                        };
                }
            }
        }

        return last ?? new CapabilityInvocationResult(
            request.InvocationId,
            capabilityId,
            Succeeded: false,
            OutputHash: null,
            ErrorCode: "CLI_NO_CAPABILITY_INVOKER",
            ErrorMessage: "No standard capability invoker was registered.",
            ReplayLogHash: null,
            Metadata: new SortedDictionary<string, string>(StringComparer.Ordinal));
    }

    private static IEnumerable<string> ExpandCapabilityAliases(string capabilityId)
    {
        yield return capabilityId;
        if (string.Equals(capabilityId, "aikernel.vfs", StringComparison.Ordinal))
        {
            yield return "aikernel.vfs.read";
        }
    }

    /// <summary>
    /// [EN] Releases the CLI service provider.
    /// [JA] CLI service provider を解放します。
    /// </summary>
    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}

internal sealed record StandardKernelCliOptions(
    string VfsRoot,
    string? SkillRoot)
{
    /// <summary>
    /// [EN] Parses shared standard-kernel CLI options.
    /// [JA] shared standard-kernel CLI option を解析します。
    /// </summary>
    public static StandardKernelCliOptions Parse(
        IReadOnlyList<string> args,
        out IReadOnlyList<string> positional)
    {
        var vfsRoot = ".";
        string? skillRoot = null;
        var remaining = new List<string>();

        for (var index = 0; index < args.Count; index++)
        {
            var current = args[index];
            if (current is "--vfs-root")
            {
                vfsRoot = ReadValue(args, ref index, current);
                continue;
            }

            if (current is "--root" or "--skill-root")
            {
                skillRoot = ReadValue(args, ref index, current);
                continue;
            }

            remaining.Add(current);
        }

        positional = remaining;
        return new StandardKernelCliOptions(Path.GetFullPath(vfsRoot), skillRoot);
    }

    private static string ReadValue(
        IReadOnlyList<string> args,
        ref int index,
        string option)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"{option} requires a value.");
        }

        return args[++index];
    }
}

internal static class CliOutput
{
    /// <summary>
    /// [EN] Prints a capability invocation result in stable key-value form.
    /// [JA] capability invocation result を stable key-value form で出力します。
    /// </summary>
    public static void PrintResult(
        CapabilityInvocationResult result)
    {
        Console.WriteLine($"invocation: {result.InvocationId}");
        Console.WriteLine($"capability: {result.CapabilityId}");
        Console.WriteLine($"succeeded: {result.Succeeded.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(result.OutputHash))
        {
            Console.WriteLine($"output_hash: {result.OutputHash}");
        }

        if (!string.IsNullOrWhiteSpace(result.ReplayLogHash))
        {
            Console.WriteLine($"replay_log_hash: {result.ReplayLogHash}");
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorCode))
        {
            Console.WriteLine($"error_code: {result.ErrorCode}");
            Console.WriteLine($"error_message: {result.ErrorMessage}");
        }

        foreach (var item in result.Metadata.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
    }

    /// <summary>
    /// [EN] Parses CLI key-value arguments into an ordinally ordered dictionary.
    /// [JA] CLI key-value argument を ordinal order の dictionary に解析します。
    /// </summary>
    public static IReadOnlyDictionary<string, string> ParseKeyValues(
        IEnumerable<string> values)
        => values
            .Select(Split)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

    private static (string Key, string Value) Split(
        string value)
    {
        var index = value.IndexOf('=', StringComparison.Ordinal);
        return index < 0
            ? (value, "")
            : (value[..index], value[(index + 1)..]);
    }
}
