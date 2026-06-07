namespace AIKernel.CLI.Commands;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.HelpCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.HelpCommand']" />
public static class HelpCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.HelpCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.HelpCommand.Run']" />
    public static int Run(string[] args)
    {
        Show();
        return 0;
    }

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.HelpCommand.Show']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.HelpCommand.Show']" />
    public static void Show()
    {
        Console.WriteLine("""
AIKernel CLI (aik)
Usage:
  aik <command> [options]

Commands:
  help        Show this help
  version     Show version
  vfs         Inspect Virtual File System
  clock       Inspect KernelClock

Examples:
  aik vfs tree
  aik clock now
""");
    }

    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.HelpCommand.Unknown']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.HelpCommand.Unknown']" />
    public static int Unknown(string cmd)
    {
        Console.WriteLine($"Unknown command: {cmd}");
        Console.WriteLine("Use 'aik help' to see available commands.");
        return 1;
    }
}
