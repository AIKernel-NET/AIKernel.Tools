using System.Text.Json;
using AIKernel.Common.IO;
using AIKernel.Common.Json;
using AIKernel.Common.Results;

namespace AIKernel.Tools.ChatHistoryScraper;

internal class Program
{
    static async Task<int> Main(string[] args)
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
            Console.WriteLine("Usage: aik-history export <url> [-o output.json]");
            return 1;
        }

        var url = args[1];
        var output = PathUtil.Normalize(
            args.Length >= 4 && args[2] == "-o" ? args[3] : "history.json"
        );

        // Try モナドで安全に処理（すべて非同期モナドに統一）
        var result =
            from history in Try.RunAsync(() => ChatHistoryScraper.ExportAsync(url))
            from json in Try.RunAsync(() => Task.FromResult(JsonSerializer.Serialize(history, JsonOptions.Indented)))
            select json;

        var final = await result; // LINQ クエリの結果は Task<Result<string>>

        if (final.IsFailure)
        {
            Console.WriteLine($"Error: {final.Error}");
            return 1;
        }

        await FileUtil.WriteTextAsync(output, final.Value!);
        Console.WriteLine($"Exported history to {output}");
        return 0;
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
