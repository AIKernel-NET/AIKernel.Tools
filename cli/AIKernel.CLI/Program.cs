using AIKernel.CLI.Commands;

namespace AIKernel.CLI;

public static class Program
{
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

            _ => HelpCommand.Unknown(command)
        };
    }
}
