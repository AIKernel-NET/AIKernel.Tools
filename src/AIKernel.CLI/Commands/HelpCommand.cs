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
  runtime     Invoke standard runtime capabilities
  system      Inspect standard provider system metadata
  capabilities
              List or invoke standard capability modules
  exec        Execute a local DSL pipeline
  skills      List, show, or invoke SKILL.md capabilities
  install     Install dynamic providers into a local provider directory
  providers   Load provider manifests, list capabilities, and invoke modules
  gpu         List and run OS compute providers
  run         Start a logical OS process
  ps          List logical OS processes
  kill        Stop a logical OS process
  restart     Restart a logical OS process
  logs        Show logical OS logs
  schedule    Manage scheduled OS commands

Examples:
  aik vfs tree
  aik clock now
  aik runtime ping
  aik system info
  aik capabilities list
  aik exec run pipeline.json input.text=hello
  aik skills list --root ./skills
  aik install provider dynamic-pipeline
  aik providers list --dir ./providers
  aik providers capabilities --dir ./providers
  aik providers invoke openai.chat chat.completion --dir ./providers prompt=hello
  aik gpu list
  aik gpu run vector-add --a a.bin --b b.bin
  aik run sample
  aik ps
  aik logs sample
  aik schedule add --every 1m "aik system info"
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
