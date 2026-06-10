using AIKernel.CLI.Commands;

namespace AIKernel.CLI;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Program']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Program']" />
public static class Program
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Program.Main']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Program.Main']" />
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            HelpCommand.Show();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        var rest = args.Skip(1).ToArray();

        return command switch
        {
            "help" => HelpCommand.Run(rest),
            "--help" => HelpCommand.Run(rest),
            "-h" => HelpCommand.Run(rest),

            "version" => VersionCommand.Run(rest),
            "--version" => VersionCommand.Run(rest),
            "-v" => VersionCommand.Run(rest),

            "vfs" => VfsCommand.Run(rest),
            "clock" => ClockCommand.Run(rest),
            "runtime" => RuntimeCommand.Run(rest),
            "system" => SystemCommand.Run(rest),
            "capabilities" => CapabilityCommand.Run(rest),
            "exec" => ExecCommand.Run(rest),
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
