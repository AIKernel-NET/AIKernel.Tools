using AIKernel.CLI.Commands;

namespace AIKernel.CLI;

/// <summary>[EN] Documents this public package API member. [JA] Program を表します。</summary>
/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Program']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Program']" />
public static class Program
{
    /// <summary>[EN] Documents this public package API member. [JA] Main を実行します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Program.Main']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Program.Main']" />
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            HelpCommand.Show();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();

        if (command == "exec")
        {
            using var cancellation = new CancellationTokenSource();
            ConsoleCancelEventHandler cancel = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
            Console.CancelKeyPress += cancel;
            try
            {
                var code = await ExecCommand.RunAsync(rest, cancellation.Token).ConfigureAwait(false);
                return cancellation.IsCancellationRequested ? 130 : code;
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { return 130; }
            finally { Console.CancelKeyPress -= cancel; }
        }
        return command switch
        {
            "help" => HelpCommand.Run(rest),
            "--help" => HelpCommand.Run(rest),
            "-h" => HelpCommand.Run(rest),

            "version" => VersionCommand.Run(rest),
            "--version" => VersionCommand.Run(rest),
            "-v" => VersionCommand.Run(rest),

            "vfs" => VfsCommand.Run(rest),
            "rom" => RomCommand.Run(rest),
            "nomos" => RomCommand.Run(rest),
            "clock" => ClockCommand.Run(rest),
            "chronos" => ClockCommand.Run(rest),
            "replay" => ClockCommand.Run(rest),
            "runtime" => RuntimeCommand.Run(rest),
            "system" => SystemCommand.Run(rest),
            "capabilities" => CapabilityCommand.Run(rest),
            "skills" => SkillCommand.Run(rest),
            "install" => InstallCommand.Run(rest),
            "providers" => ProviderCommand.Run(rest),
            "gpu" => GpuCommand.Run(rest),
            "run" => ProcessCommand.RunProcess(rest),
            "ps" => ProcessCommand.Ps(rest),
            "kill" => ProcessCommand.Kill(rest),
            "restart" => ProcessCommand.Restart(rest),
            "logs" => LogsCommand.Run(rest),
            "schedule" => ScheduleCommand.Run(rest),

            _ => HelpCommand.Unknown(command)
        };
    }
}
