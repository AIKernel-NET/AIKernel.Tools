using AIKernel.Providers.ChatHistory;
using AIKernel.Tools.Instrumentation.Concepts;
using AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;
using System.Text.Json;

namespace AIKernel.CLI.Commands;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.RomCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.RomCommand']" />
public static class RomCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.RomCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.RomCommand.Run']" />
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Usage();
            return 1;
        }

        var sub = args[0].ToLowerInvariant();
        if (sub == "view")
        {
            return View();
        }

        if (sub != "build")
        {
            Console.WriteLine($"Unknown rom command: {sub}");
            return 1;
        }

        if (args.Length < 2)
        {
            Usage();
            return 1;
        }

        var input = args[1];
        var format = args.Length >= 4 && args[2] == "--format"
            ? args[3]
            : "rom";

        if (!File.Exists(input))
        {
            Console.WriteLine($"Input file not found: {input}");
            return 1;
        }

        var json = File.ReadAllText(input);
        var records = JsonSerializer.Deserialize<List<ChatHistoryRecord>>(json)
                      ?? [];

        string output = format switch
        {
            "rom" => RomExporter.ToRom(records),
            "md" => MdExporter.ToMarkdown(records),
            _ => throw new InvalidOperationException($"Unknown format: {format}")
        };

        var outPath = Path.ChangeExtension(input, format);
        File.WriteAllText(outPath, output);

        Console.WriteLine($"Generated: {outPath}");
        return 0;
    }

    private static int View()
    {
        var viewer = new NomosViewer();

        Console.WriteLine("rom.viewer: nomos");
        Console.WriteLine("rom.command: aik rom build <input.json> [--format rom|md]");
        Console.WriteLine($"rom.alias: {viewer.Alias()}");
        Console.WriteLine($"rom.alias: {viewer.Alias("nomos")}");
        return 0;
    }

    private static void Usage()
    {
        Console.WriteLine("Usage: aik rom <view|build>");
        Console.WriteLine("       aik rom build <input.json> [--format rom|md]");
        Console.WriteLine("       aik rom view");
        Console.WriteLine("       aik nomos view");
    }
}
