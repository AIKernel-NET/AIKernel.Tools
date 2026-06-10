using AIKernel.Tools.Instrumentation;

namespace AIKernel.Tools.Tests;

public sealed class InstrumentationSurfaceTests
{
    [Fact]
    public void CanonicalFormatterOrdersDictionaryKeysDeterministically()
    {
        var formatter = new CanonicalFormatter();
        var value = new Dictionary<string, object?>
        {
            ["z"] = 2,
            ["a"] = 1
        };

        Assert.Equal("{a: 1, z: 2}", formatter.Serialize(value));
        Assert.Equal("{a: 1, z: 2}", CanonicalSerializer.Serialize(value));
    }

    [Fact]
    public void InspectorProducesDeterministicDiffSummary()
    {
        var inspector = new Inspector();

        var diff = inspector.Diff("left", "right");

        Assert.Contains("equal: false", diff);
        Assert.Contains("left: left", diff);
        Assert.Contains("right: right", diff);
    }

    [Fact]
    public void ReplayEngineLoadsStepsAndRunsLineReplayMaterial()
    {
        var path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "aikernel-tools-tests",
            Guid.NewGuid().ToString("N"),
            "replay.log");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        try
        {
            File.WriteAllLines(path, ["first", "second"]);
            var engine = new ReplayEngine().Load(path);

            Assert.Equal("Loaded", engine.Session().State);
            Assert.Equal("first", engine.Step());
            Assert.Equal("Running", engine.Session().State);
            Assert.Equal("Completed", engine.Run().State);
            Assert.Equal("2", engine.Session().Metadata["event_count"]);
        }
        finally
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (directory is not null && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
