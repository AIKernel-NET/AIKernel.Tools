using System.Text.Json;
using AIKernel.Common.Results;
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
        var output = Path.GetFullPath(OutputPath(args));

        return (await Try.RunAsync(async () =>
        {
            var history = await ChatHistoryScraper.ExportAsync(url).ConfigureAwait(false);
            var records = ChatHistoryScraper.ToRecords(history);

            string text = output.ToLowerInvariant() switch
            {
                var p when p.EndsWith(".rom") => RomExporter.ToRom(records),
                var p when p.EndsWith(".md") => MdExporter.ToMarkdown(records),
                _ => JsonSerializer.Serialize(
                    records,
                    new JsonSerializerOptions { WriteIndented = true })
            };

            await File.WriteAllTextAsync(output, text).ConfigureAwait(false);
            Console.WriteLine($"Exported history to {output}");
            return 0;
        }).ConfigureAwait(false)).Match(
            error =>
            {
                Console.Error.WriteLine(error.Message);
                return 1;
            },
            exitCode => exitCode);
    }

    private static string OutputPath(string[] args)
        => OutputPathOption(args).Match(() => "history.rom", value => value);

    private static Option<string> OutputPathOption(string[] args)
        => args.Length >= 4 && args[2] == "-o"
            ? Option<string>.Some(args[3])
            : Option<string>.None();

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
