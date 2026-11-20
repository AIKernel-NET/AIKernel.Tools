namespace AIKernel.Tools.Tests;

using System.Buffers.Binary;
using System.IO.Compression;
using AIKernel.CLI;

public sealed class CudaDriverVerificationTests
{
    private static int RunCli(string[] args) => Program.Main(args).GetAwaiter().GetResult();
    [Fact]
    public void HelpIncludesDriverVerificationWithoutRemovingLegacyCommands()
    {
        var (code, output) = Run("help");
        Assert.Equal(0, code);
        Assert.Contains("--provider cuda-driver --package", output);
        Assert.Contains("--provider cuda13 --package", output);
    }

    [Theory]
    [InlineData("valid", 0)]
    [InlineData("missing-windows", 1)]
    [InlineData("missing-linux", 1)]
    [InlineData("empty", 1)]
    [InlineData("wrong-architecture", 1)]
    [InlineData("truncated", 1)]
    [InlineData("duplicate", 1)]
    [InlineData("torch", 1)]
    public void PackageRequiresBothRuntimeImages(string variant, int expected)
    {
        var path = Path.Combine(Path.GetTempPath(), "aik-cuda-driver-" + Guid.NewGuid().ToString("N") + ".nupkg");
        try
        {
            using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                var pe = new byte[128]; pe[0] = (byte)'M'; pe[1] = (byte)'Z'; pe[60] = 64;
                pe[64] = (byte)'P'; pe[65] = (byte)'E';
                BinaryPrimitives.WriteUInt16LittleEndian(pe.AsSpan(68), variant == "wrong-architecture" ? (ushort)0x14c : (ushort)0x8664);
                var elf = new byte[64]; elf[0] = 0x7f; elf[1] = (byte)'E'; elf[2] = (byte)'L'; elf[3] = (byte)'F'; elf[4] = 2; elf[5] = 1; elf[18] = 62;
                if (variant != "missing-windows") Add("runtimes/win-x64/native/aikernel_cuda_driver.dll", variant == "empty" ? [] : variant == "truncated" ? pe[..20] : pe);
                if (variant != "missing-linux") Add("runtimes/linux-x64/native/libaikernel_cuda_driver.so", elf);
                Add("lib/net10.0/AIKernel.Kernel.dll", [1]);
                Add("licenses/cuda-driver/LICENSE", [1]);
                if (variant == "duplicate") Add("runtimes/linux-x64/native/libaikernel_cuda_driver.so", elf);
                if (variant == "torch") Add("runtimes/win-x64/native/libtorch_bridge.dll", [1]);
                void Add(string name, byte[] bytes) { using var stream = zip.CreateEntry(name).Open(); stream.Write(bytes); }
            }
            var (code, output) = Run("gpu", "verify-native", "--provider", "cuda-driver", "--package", path);
            Assert.Equal(expected, code);
            Assert.Equal(expected == 0, output.Contains("native-artifact-integrity: ok", StringComparison.Ordinal));
            Assert.DoesNotContain("gpu-test: passed", output);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrInvalidLibraryCannotSucceed(bool exists)
    {
        var path = Path.Combine(Path.GetTempPath(), "aik-invalid-driver-" + Guid.NewGuid().ToString("N") + ".dll");
        try
        {
            if (exists) File.WriteAllText(path, "not a native library");
            var (code, output) = Run("gpu", "verify-native", "--provider", "cuda-driver", "--library", path);
            Assert.Equal(1, code);
            Assert.DoesNotContain("native-artifact-integrity: ok", output);
            Assert.DoesNotContain("AIKERNEL_LIBTORCH_PATH", output);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private static (int, string) Run(params string[] args)
    {
        var original = Console.Out;
        using var output = new StringWriter();
        try { Console.SetOut(output); return (RunCli(args), output.ToString()); }
        finally { Console.SetOut(original); }
    }
}
