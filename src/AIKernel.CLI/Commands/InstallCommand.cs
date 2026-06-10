namespace AIKernel.CLI.Commands;

using AIKernel.Common.Results;

/// <summary>
/// [EN] CLI command for installing dynamic provider manifests into a local provider directory.
/// [JA] dynamic provider manifest を local provider directory へ install する CLI command です。
/// </summary>
public static class InstallCommand
{
    private static readonly IReadOnlyDictionary<string, ProviderInstallSpec> Providers =
        new Dictionary<string, ProviderInstallSpec>(StringComparer.OrdinalIgnoreCase)
        {
            ["openai"] = new("src/Llm/ChatOpenAIProvider", "openai.provider.json"),
            ["chat-openai"] = new("src/Llm/ChatOpenAIProvider", "openai.provider.json"),
            ["chat-history"] = new("src/Chat/ChatHistoryProvider", "chat-history.provider.json"),
            ["cuda"] = new("src/Compute/CudaComputeProvider", "cuda.provider.json"),
            ["cuda-compute"] = new("src/Compute/CudaComputeProvider", "cuda.provider.json"),
            ["dynamic-pipeline"] = new("src/Pipeline/DynamicPipelineCompilerProvider", "dynamic-pipeline.provider.json"),
            ["local-llm"] = new("src/Llm/LocalLlmProvider", "local-llm.provider.json")
        };

    /// <summary>
    /// [EN] Executes the install command.
    /// [JA] install command を実行します。
    /// </summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            ShowHelp();
            return 0;
        }

        if (!string.Equals(args[0], "provider", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Unknown install target: {args[0]}");
            ShowHelp();
            return 1;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: aik install provider <name> [--dir ./providers]");
            return 1;
        }

        return Try.Run(() => InstallOptions.Parse(args.Skip(2).ToArray()))
            .Bind(options => Try.Run(() => InstallProvider(args[1], options)))
            .Match(
                error =>
                {
                    Console.WriteLine($"Provider install failed: {error.Message}");
                    return 1;
                },
                exitCode => exitCode);
    }

    private static int InstallProvider(
        string name,
        InstallOptions options)
    {
        if (!Providers.TryGetValue(name, out var spec))
        {
            Console.WriteLine($"Unknown provider: {name}");
            Console.WriteLine("Known providers: " + string.Join(", ", Providers.Keys.OrderBy(x => x, StringComparer.Ordinal)));
            return 1;
        }

        var sourceRoot = ResolveProviderSourceRoot(options.Source);
        var projectRoot = sourceRoot / spec.ProjectDirectory;
        var outputRoot = ResolveBuildOutput(projectRoot);
        var destination = Path.GetFullPath(Path.Combine(options.Directory, name));
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.EnumerateFiles(outputRoot, "*.dll"))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }

        var manifest = outputRoot / spec.ManifestName;
        if (!File.Exists(manifest))
        {
            manifest = projectRoot / spec.ManifestName;
        }

        var manifestValidation = ValidateFileExists(manifest, $"Provider manifest was not found: {spec.ManifestName}");
        if (manifestValidation.Match(_ => true, _ => false))
        {
            Console.WriteLine(manifestValidation.Match(error => error.Message, _ => string.Empty));
            return 1;
        }

        File.Copy(manifest, Path.Combine(destination, spec.ManifestName), overwrite: true);
        Console.WriteLine($"Installed provider '{name}' to {destination}");
        return 0;
    }

    private static PathInfo ResolveProviderSourceRoot(string? configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return new PathInfo(Path.GetFullPath(configured));
        }

        var environment = Environment.GetEnvironmentVariable("AIKERNEL_PROVIDER_SOURCE");
        if (!string.IsNullOrWhiteSpace(environment))
        {
            return new PathInfo(Path.GetFullPath(environment));
        }

        var baseDirectory = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "..", "AIKernel.Providers")),
            Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "AIKernel.Providers")),
            Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "AIKernel.Providers"))
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return new PathInfo(candidate);
            }
        }

        throw new DirectoryNotFoundException("AIKernel.Providers source root was not found. Use --source or AIKERNEL_PROVIDER_SOURCE.");
    }

    private static Result<bool> ValidateFileExists(string path, string message)
        => File.Exists(path)
            ? Result<bool>.Success(true)
            : Result<bool>.Fail(message);

    private static PathInfo ResolveBuildOutput(PathInfo projectRoot)
    {
        var candidates = new[]
        {
            projectRoot / "bin" / "Release" / "net10.0",
            projectRoot / "bin" / "Debug" / "net10.0"
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException($"Provider build output was not found for {projectRoot}. Build AIKernel.Providers first.");
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
AIKernel install commands
Usage:
  aik install provider <name> [--dir ./providers] [--source ../AIKernel.Providers]

Examples:
  aik install provider dynamic-pipeline
  aik install provider cuda --dir ./providers
""");
    }
}

internal sealed record ProviderInstallSpec(
    string ProjectDirectory,
    string ManifestName);

internal sealed record InstallOptions(
    string Directory,
    string? Source)
{
    /// <summary>
    /// [EN] Parses provider install command options.
    /// [JA] provider install command option を解析します。
    /// </summary>
    public static InstallOptions Parse(string[] args)
    {
        var directory = "./providers";
        string? source = null;

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

            if (current is "--source" or "-s")
            {
                if (index + 1 >= args.Length)
                {
                    throw new ArgumentException("--source requires a path.");
                }

                source = args[++index];
                continue;
            }
        }

        return new InstallOptions(directory, source);
    }
}

internal readonly record struct PathInfo(string Value)
{
    /// <summary>
    /// [EN] Converts a path wrapper to its string value.
    /// [JA] path wrapper を string value に変換します。
    /// </summary>
    public static implicit operator string(PathInfo path) => path.Value;

    /// <summary>
    /// [EN] Combines two path segments in a platform-aware manner.
    /// [JA] 2 つの path segment を platform-aware に結合します。
    /// </summary>
    public static PathInfo operator /(PathInfo left, string right)
        => new(Path.Combine(left.Value, right));

    /// <summary>
    /// [EN] Returns the wrapped path value.
    /// [JA] wrap された path value を返します。
    /// </summary>
    public override string ToString() => Value;
}
