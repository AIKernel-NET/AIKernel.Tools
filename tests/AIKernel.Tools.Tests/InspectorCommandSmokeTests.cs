using AIKernel.Tools.Inspectors.KernelClock.Commands;
using AIKernel.Tools.Inspectors.Vfs.Commands;

namespace AIKernel.Tools.Tests;

public sealed class InspectorCommandSmokeTests
{
    [Fact]
    public void VfsTreeCommandPrintsBoundedDirectoryTree()
    {
        using var workspace = TemporaryWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Path, "sample.txt"), "hello");

        var output = Capture(() => TreeCommand.Run(workspace.Path));

        Assert.Contains(workspace.Path, output);
        Assert.Contains("[F] sample.txt", output);
        Assert.DoesNotContain("not implemented", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VfsInfoCommandPrintsDirectoryMetadata()
    {
        using var workspace = TemporaryWorkspace.Create();
        File.WriteAllText(Path.Combine(workspace.Path, "sample.txt"), "hello");

        var output = Capture(() => InfoCommand.Run(workspace.Path));

        Assert.Contains("type: directory", output);
        Assert.Contains("entries: 1", output);
        Assert.DoesNotContain("not implemented", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void KernelClockNowCommandPrintsStructuredClockSnapshot()
    {
        var output = Capture(NowCommand.Run);

        Assert.Contains("kernel_clock.utc:", output);
        Assert.Contains("kernel_clock.unix_ms:", output);
        Assert.DoesNotContain("mock", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void KernelClockTimelineCommandPrintsStructuredEventSnapshot()
    {
        var output = Capture(TimelineCommand.Run);

        Assert.Contains("timeline[0].event: kernel.clock.inspect", output);
        Assert.Contains("timeline[0].observed_utc:", output);
        Assert.Contains("timeline[0].unix_ms:", output);
        Assert.DoesNotContain("not implemented", output, StringComparison.OrdinalIgnoreCase);
    }

    private static string Capture(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        try
        {
            Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(original);
        }
    }

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
                "aikernel-tools-tests",
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
