using AIKernel.CLI;

namespace AIKernel.Tools.Tests;

public sealed class CliCommandSmokeTests
{
    [Fact]
    public void RuntimePingInvokesStandardMinimalRuntimeProvider()
    {
        var (exitCode, output) = Capture(() => Program.Main(["runtime", "ping"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("capability: aikernel.runtime.ping", output);
        Assert.Contains("status: ok", output);
    }

    [Fact]
    public void SystemInfoPrintsStandardProviderSnapshot()
    {
        var (exitCode, output) = Capture(() => Program.Main(["system", "info"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("\"providerCount\":", output);
        Assert.Contains("\"capabilityCount\":", output);
    }

    [Fact]
    public void CapabilitiesListShowsStandardCapabilityModules()
    {
        var (exitCode, output) = Capture(() => Program.Main(["capabilities", "list"]));

        Assert.Equal(0, exitCode);
        Assert.Contains("aikernel.runtime.ping", output);
        Assert.Contains("aikernel.system.info", output);
        Assert.Contains("aikernel.local.execute", output);
    }

    [Fact]
    public void ExecRunInvokesLocalExecutionProvider()
    {
        using var workspace = TemporaryWorkspace.Create();
        var pipelinePath = Path.Combine(workspace.Path, "pipeline.json");
        File.WriteAllText(
            pipelinePath,
            """
            {
              "type": "Pipeline",
              "steps": [
                { "type": "Step", "name": "start" }
              ]
            }
            """);

        var (exitCode, output) = Capture(() => Program.Main(["exec", "run", pipelinePath]));

        Assert.Equal(0, exitCode);
        Assert.Contains("capability: aikernel.local.execute", output);
        Assert.Contains("dsl.status: Succeeded", output);
    }

    [Fact]
    public void ProcessAndLogsCommandsExposeOsSurface()
    {
        var (runExit, runOutput) = Capture(() => Program.Main(["run", "sample"]));
        var (psExit, psOutput) = Capture(() => Program.Main(["ps"]));
        var (logsExit, logsOutput) = Capture(() => Program.Main(["logs", "sample"]));

        Assert.Equal(0, runExit);
        Assert.Equal(0, psExit);
        Assert.Equal(0, logsExit);
        Assert.Contains("process: sample", runOutput);
        Assert.Contains("Running sample", psOutput);
        Assert.Contains("sample: ProcessStarted", logsOutput);
    }

    [Fact]
    public void RunHaloWorldWasmCompletesAndWritesLogs()
    {
        using var workspace = TemporaryWorkspace.Create();
        var wasmPath = Path.Combine(workspace.Path, "HaloWorld.wasm");
        File.WriteAllBytes(wasmPath, Convert.FromBase64String("AGFzbQEAAAA="));

        var (runExit, runOutput) = Capture(() => Program.Main(["run", "haloworld", "--wasm", wasmPath]));
        var (psExit, psOutput) = Capture(() => Program.Main(["ps"]));
        var (logsExit, logsOutput) = Capture(() => Program.Main(["logs", "haloworld"]));

        Assert.Equal(0, runExit);
        Assert.Equal(0, psExit);
        Assert.Equal(0, logsExit);
        Assert.Contains("process: haloworld", runOutput);
        Assert.Contains("state: Stopped", runOutput);
        Assert.Contains("stdout: HaloWorld", runOutput);
        Assert.Contains("Stopped haloworld", psOutput);
        Assert.Contains("haloworld: Stdout HaloWorld", logsOutput);
        Assert.Contains("haloworld: ProcessStopped", logsOutput);
    }

    [Fact]
    public void GpuRunVectorAddWritesOutput()
    {
        using var workspace = TemporaryWorkspace.Create();
        var leftPath = Path.Combine(workspace.Path, "a.bin");
        var rightPath = Path.Combine(workspace.Path, "b.bin");
        var outputPath = Path.Combine(workspace.Path, "out.bin");
        File.WriteAllBytes(leftPath, FloatsToBytes([1.0f, 2.0f]));
        File.WriteAllBytes(rightPath, FloatsToBytes([3.0f, 4.0f]));

        var (exitCode, output) = Capture(() => Program.Main([
            "gpu", "run", "vector-add",
            "--a", leftPath,
            "--b", rightPath,
            "--out", outputPath
        ]));

        Assert.Equal(0, exitCode);
        Assert.Contains("operation: vector-add", output);
        Assert.Equal([4.0f, 6.0f], BytesToFloats(File.ReadAllBytes(outputPath)));
    }

    [Fact]
    public void ScheduleCommandAddsAndListsCommand()
    {
        var (addExit, addOutput) = Capture(() => Program.Main(["schedule", "add", "--every", "1s", "aik gpu run vector-add"]));
        var (listExit, listOutput) = Capture(() => Program.Main(["schedule", "list"]));

        Assert.Equal(0, addExit);
        Assert.Equal(0, listExit);
        Assert.Contains("every: 1s", addOutput);
        Assert.Contains("aik gpu run vector-add", listOutput);
    }

    private static (int ExitCode, string Output) Capture(Func<int> action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        try
        {
            Console.SetOut(writer);
            var exitCode = action();
            return (exitCode, writer.ToString());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    private static byte[] FloatsToBytes(float[] values)
        => System.Runtime.InteropServices.MemoryMarshal.AsBytes<float>(values.AsSpan()).ToArray();

    private static float[] BytesToFloats(byte[] values)
        => System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(values).ToArray();

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryWorkspace Create()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "aikernel-tools-cli-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TemporaryWorkspace(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
