using AIKernel.Tools.Capability.RomStorage.ChatHistory;
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
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: aik rom build <input.json> [--format rom|md]");
            return 1;
        }

        var sub = args[0];
        if (sub != "build")
        {
            Console.WriteLine($"Unknown rom command: {sub}");
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
}
