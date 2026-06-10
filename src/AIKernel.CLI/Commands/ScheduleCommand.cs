namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;

/// <summary>
/// [EN] Handles logical OS scheduler commands.
/// [JA] logical OS scheduler command を処理します。
/// </summary>
public static class ScheduleCommand
{
    /// <summary>[EN] Runs a scheduler command. [JA] scheduler command を実行します。</summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            ShowUsage();
            return 0;
        }

        return args[0].ToLowerInvariant() switch
        {
            "add" => Add(args.Skip(1).ToArray()),
            "list" => List(),
            _ => HelpCommand.Unknown("schedule " + args[0])
        };
    }

    private static int Add(string[] args)
    {
        var every = "";
        var commandParts = new List<string>();
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] == "--every" && index + 1 < args.Length)
            {
                every = args[++index];
                continue;
            }

            commandParts.Add(args[index]);
        }

        if (string.IsNullOrWhiteSpace(every) || commandParts.Count == 0)
        {
            Console.WriteLine("Usage: aik schedule add --every 1m \"aik system info\"");
            return 1;
        }

        var schedule = CliOsState.Schedule(every, string.Join(" ", commandParts));
        Console.WriteLine($"schedule: {schedule.Id}");
        Console.WriteLine($"every: {schedule.Every}");
        Console.WriteLine($"command: {schedule.Command}");
        return 0;
    }

    private static int List()
    {
        foreach (var schedule in CliOsState.ListSchedules())
        {
            Console.WriteLine($"{schedule.Id} every={schedule.Every} command={schedule.Command}");
        }

        return 0;
    }

    private static void ShowUsage()
        => Console.WriteLine("""
Usage:
  aik schedule add --every 1m "aik system info"
  aik schedule list
""");
}
