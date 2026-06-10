namespace AIKernel.CLI.Commands;

using AIKernel.Common.Results;
using System.Runtime.InteropServices;

/// <summary>
/// [EN] Handles OS GPU commands.
/// [JA] OS GPU command を処理します。
/// </summary>
public static class GpuCommand
{
    /// <summary>[EN] Runs a GPU command. [JA] GPU command を実行します。</summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            ShowUsage();
            return 0;
        }

        var result = args[0].ToLowerInvariant() switch
        {
            "list" => Result<int>.Success(List()),
            "run" => RunKernel(args.Skip(1).ToArray()),
            _ => Result<int>.Success(HelpCommand.Unknown("gpu " + args[0]))
        };
        return ExitCode(result, "GPU command failed");
    }

    private static int List()
    {
        Console.WriteLine("provider: cpu");
        Console.WriteLine("provider: webgpu");
        Console.WriteLine("fallback: cpu");
        return 0;
    }

    private static Result<int> RunKernel(string[] args)
    {
        if (args.Length == 0 || !string.Equals(args[0], "vector-add", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: aik gpu run vector-add --a a.bin --b b.bin [--out out.bin]");
            return Result<int>.Success(1);
        }

        var options = ParseOptions(args.Skip(1).ToArray());
        return
            from paths in ValidateVectorAddPaths(options)
            from left in Try.Run(() => ReadFloatVector(paths.LeftPath))
            from right in Try.Run(() => ReadFloatVector(paths.RightPath))
            from _ in ValidateEqualLength(left, right)
            from output in Result<float[]>.Success(AddVectors(left, right))
            from written in Try.Run(() =>
            {
                File.WriteAllBytes(paths.OutputPath, MemoryMarshal.AsBytes<float>(output.AsSpan()).ToArray());
                return true;
            })
            select PrintVectorAdd(paths.OutputPath, output.Length);
    }

    private static Result<VectorAddPaths> ValidateVectorAddPaths(IReadOnlyDictionary<string, string> options)
    {
        if (!options.TryGetValue("--a", out var aPath) || !options.TryGetValue("--b", out var bPath))
        {
            return Result<VectorAddPaths>.Fail("Usage: aik gpu run vector-add --a a.bin --b b.bin [--out out.bin]");
        }

        var outPath = options.TryGetValue("--out", out var configuredOut)
            ? configuredOut
            : Path.Combine(Environment.CurrentDirectory, "vector-add.out.bin");
        return Result<VectorAddPaths>.Success(new VectorAddPaths(aPath, bPath, outPath));
    }

    private static float[] ReadFloatVector(string path)
        => MemoryMarshal.Cast<byte, float>(File.ReadAllBytes(path)).ToArray();

    private static Result<bool> ValidateEqualLength(IReadOnlyList<float> left, IReadOnlyList<float> right)
        => left.Count == right.Count
            ? Result<bool>.Success(true)
            : Result<bool>.Fail("input vector lengths must match. ErrorCode=CLI_GPU_VECTOR_LENGTH_MISMATCH");

    private static float[] AddVectors(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        var output = new float[left.Count];
        for (var index = 0; index < output.Length; index++)
        {
            output[index] = left[index] + right[index];
        }

        return output;
    }

    private static int PrintVectorAdd(string outPath, int elementCount)
    {
        Console.WriteLine("provider: cpu-fallback");
        Console.WriteLine("operation: vector-add");
        Console.WriteLine($"elements: {elementCount}");
        Console.WriteLine($"output: {outPath}");
        return 0;
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index].StartsWith("--", StringComparison.Ordinal) && index + 1 < args.Length)
            {
                options[args[index]] = args[++index];
            }
        }

        return options;
    }

    private static void ShowUsage()
        => Console.WriteLine("""
Usage:
  aik gpu list
  aik gpu run vector-add --a a.bin --b b.bin [--out out.bin]
""");

    private static int ExitCode(Result<int> result, string prefix)
        => result.Match(
            error =>
            {
                Console.WriteLine($"{prefix}: {error.Message}");
                return 1;
            },
            value => value);

    private sealed record VectorAddPaths(string LeftPath, string RightPath, string OutputPath);
}
