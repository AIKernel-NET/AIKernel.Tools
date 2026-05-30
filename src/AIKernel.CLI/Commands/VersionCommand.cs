namespace AIKernel.CLI.Commands;

public static class VersionCommand
{
    public static int Run(string[] args)
    {
        Console.WriteLine("aik version 0.1.0");
        return 0;
    }
}
