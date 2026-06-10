namespace AIKernel.CLI.Commands;

using AIKernel.CLI.Services;

/// <summary>
/// [EN] Handles logical OS log commands.
/// [JA] logical OS log command を処理します。
/// </summary>
public static class LogsCommand
{
    /// <summary>[EN] Prints logs for a logical process. [JA] logical process の log を出力します。</summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik logs <process>");
            return 1;
        }

        foreach (var line in CliOsState.Logs(args[0]))
        {
            Console.WriteLine(line);
        }

        return 0;
    }
}
