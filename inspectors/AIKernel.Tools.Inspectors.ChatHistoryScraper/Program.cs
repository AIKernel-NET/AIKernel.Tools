using AIKernel.Common.IO;
using AIKernel.Common.Json;
using AIKernel.Tools.Inspectors.ChatHistoryScraper.Export;

namespace AIKernel.Tools.Inspectors.ChatHistoryScraper;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 1;
        }

        var command = args[0].ToLowerInvariant();
        return command switch
        {
            "export" => await RunExportAsync(args),
            "help" => PrintHelp(),
            _ => UnknownCommand(command)
        };
    }

    private static async Task<int> RunExportAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: aik-history export <url> [-o output.json|output.md|output.rom]");
            return 1;
        }

        var url = args[1];
        var output = PathUtil.Normalize(
            args.Length >= 4 && args[2] == "-o" ? args[3] : "history.rom");

        try
        {
            var history = await ChatHistoryScraper.ExportAsync(url);
            var records = ChatHistoryScraper.ToRecords(history);

            string text = output.ToLowerInvariant() switch
            {
                var p when p.EndsWith(".rom") => RomExporter.ToRom(records),
                var p when p.EndsWith(".md") => MdExporter.ToMarkdown(records),
                _ => System.Text.Json.JsonSerializer.Serialize(records, JsonOptions.Indented)
            };

            await FileUtil.WriteTextAsync(output, text);
            Console.WriteLine($"Exported history to {output}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int PrintHelp()
    {
        Console.WriteLine("""
AIKernel.Tools.Inspectors.ChatHistoryScraper CLI

Commands:
  export <url> [-o file]   Export chat history from shared URL
                           Supported formats: .json, .md, .rom
                           .rom emits signed AIKernel HistoryROM Markdown
  help                     Show this help
""");
        return 0;
    }

    private static int UnknownCommand(string cmd)
    {
        Console.WriteLine($"Unknown command: {cmd}");
        Console.WriteLine("Use 'help' for usage.");
        return 1;
    }
}
