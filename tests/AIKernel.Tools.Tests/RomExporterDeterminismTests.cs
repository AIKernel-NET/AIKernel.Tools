using AIKernel.Providers.ChatHistory;
using AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;

namespace AIKernel.Tools.Tests;

public sealed class RomExporterDeterminismTests
{
    private const string AgentPlanHash =
        "sha256:a38cfaeb3485a0dda72d87e982288f506a133fa779907d93a584ec6ac98010f6";

    [Fact]
    public void ToRomProducesDeterministicMarkdownForIdenticalInputs()
    {
        var records = Records();
        var generatedAt = new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero);

        var first = RomExporter.ToRom(records, "agent tools", "plan/one", generatedAt);
        var second = RomExporter.ToRom(records, "agent tools", "plan/one", generatedAt);

        Assert.Equal(first, second);
        Assert.Contains("rom_id: 'history://agent-tools/plan-one'", first);
        Assert.Contains("generated_at: '2026-06-08T01:02:03.0000000+00:00'", first);
        Assert.Contains($"  hash: '{AgentPlanHash}'", first);
    }

    [Fact]
    public void ToRomFixesYamlHeaderOrderAndHashValue()
    {
        var rom = RomExporter.ToRom(
            Records(),
            "agent tools",
            "plan/one",
            new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero));

        var header = rom.Split('\n').Take(14).ToArray();

        Assert.Equal(
        [
            "---",
            "rom_id: 'history://agent-tools/plan-one'",
            "entity_type: 'conversation'",
            "version: '1'",
            "source_kind: 'chat_history'",
            "generated_at: '2026-06-08T01:02:03.0000000+00:00'",
            "security:",
            "  tags:",
            "    - 'chat'",
            "    - 'history'",
            "    - 'scraped'",
            "signature:",
            $"  hash: '{AgentPlanHash}'",
            "---"
        ], header);
    }

    [Fact]
    public void ToRomOrdersSecurityTagsAndTurnsDeterministically()
    {
        var rom = RomExporter.ToRom(
            Records(),
            "scraper",
            "history",
            new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero));

        Assert.True(
            rom.IndexOf("    - 'chat'", StringComparison.Ordinal)
            < rom.IndexOf("    - 'history'", StringComparison.Ordinal));
        Assert.True(
            rom.IndexOf("    - 'history'", StringComparison.Ordinal)
            < rom.IndexOf("    - 'scraped'", StringComparison.Ordinal));
        Assert.Equal(1, CountOccurrences(rom, "    - 'chat'\n"));
        Assert.Equal(1, CountOccurrences(rom, "    - 'history'\n"));
        Assert.Equal(1, CountOccurrences(rom, "    - 'scraped'\n"));
        Assert.True(
            rom.IndexOf("## Turn:1", StringComparison.Ordinal)
            < rom.IndexOf("## Turn:2", StringComparison.Ordinal));
        Assert.Contains("@role: user", rom);
        Assert.Contains("@role: assistant", rom);
    }

    [Fact]
    public void ToRomInfersGeneratedAtFromLatestRecordDeterministically()
    {
        var rom = RomExporter.ToRom(Records(), "scraper", "history");

        Assert.Contains("generated_at: '2026-06-08T01:02:03.0000000+00:00'", rom);
    }

    [Fact]
    public void ToRomNormalizesContentNewlinesAndKeepsLfOnlyOutput()
    {
        var rom = RomExporter.ToRom(
            Records(),
            "scraper",
            "history",
            new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero));

        Assert.DoesNotContain("\r", rom);
        Assert.Contains("\nHello\n\n## Turn:2", rom);
        Assert.Contains("\nHi\nthere\n", rom);
        Assert.DoesNotContain("Hello\r\n", rom);
    }

    [Fact]
    public void ToRomPreservesMarkdownBodySpecialCharactersDeterministically()
    {
        var records = new[]
        {
            new ChatHistoryRecord
            {
                Role = "assistant:planner",
                Content = "hello: 'world'\rnext",
                Timestamp = new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero)
            }
        };

        var first = RomExporter.ToRom(
            records,
            "planner",
            "quote-test",
            new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero));
        var second = RomExporter.ToRom(
            records,
            "planner",
            "quote-test",
            new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero));

        Assert.Equal(first, second);
        Assert.Contains("@role: assistant:planner\n", first);
        Assert.Contains("hello: 'world'\nnext\n", first);
    }

    [Fact]
    public void ToRomSanitizesNamespaceAndNameDeterministically()
    {
        var rom = RomExporter.ToRom(
            Records(),
            " agent tools ",
            "plan/one",
            new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero));

        Assert.Contains("rom_id: 'history://agent-tools/plan-one'", rom);
        Assert.DoesNotContain("agent tools", rom);
        Assert.DoesNotContain("plan/one", rom);
    }

    private static ChatHistoryRecord[] Records()
        =>
        [
            new()
            {
                Role = " user ",
                Content = "Hello\r\n",
                Timestamp = new DateTimeOffset(2026, 6, 8, 1, 0, 0, TimeSpan.Zero)
            },
            new()
            {
                Role = "assistant",
                Content = "Hi\nthere",
                Timestamp = new DateTimeOffset(2026, 6, 8, 1, 2, 3, TimeSpan.Zero)
            }
        ];

    private static int CountOccurrences(
        string value,
        string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }
}
