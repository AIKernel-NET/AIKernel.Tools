using AIKernel.Tools.CapabilityModules.ChatHistoryCapability;
using AIKernel.Tools.ChatHistoryScraper.Export;
using System.Text.Json;

namespace AIKernel.CLI.Commands;

public static class RomCommand
{
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
