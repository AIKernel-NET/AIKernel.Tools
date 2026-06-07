namespace AIKernel.CLI.Commands;

public static class HelpCommand
{
    public static int Run(string[] args)
    {
        Show();
        return 0;
    }

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

    public static int Unknown(string cmd)
    {
        Console.WriteLine($"Unknown command: {cmd}");
        Console.WriteLine("Use 'aik help' to see available commands.");
        return 1;
    }
}
