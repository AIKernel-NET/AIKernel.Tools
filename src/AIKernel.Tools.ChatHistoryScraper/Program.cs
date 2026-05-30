using AIKernel.Common.IO;
using AIKernel.Common.Json;

namespace AIKernel.Tools.ChatHistoryScraper;

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
            Console.WriteLine("Usage: aik-history export <url> [-o output.json|output.md]");
            return 1;
        }

        var url = args[1];
        var output = PathUtil.Normalize(
            args.Length >= 4 && args[2] == "-o" ? args[3] : "history.md");

        try
        {
            var history = await ChatHistoryScraper.ExportAsync(url);
            var text = output.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                ? ChatHistoryScraper.ToMarkdown(history, url)
                : System.Text.Json.JsonSerializer.Serialize(history, JsonOptions.Indented);

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
AIKernel.Tools.ChatHistoryScraper CLI

Commands:
  export <url> [-o file]   Export chat history from shared URL
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
