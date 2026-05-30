namespace AIKernel.CLI.Commands;

public static class VfsCommand
{
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik vfs <tree|info>");
            return 1;
        }

        return args[0] switch
        {
            "tree" => Tree(),
            "info" => Info(),
            _ => Unknown(args[0])
        };
    }

    private static int Tree()
    {
        Console.WriteLine("[VFS] (MVP) Tree view is not implemented yet.");
        return 0;
    }

    private static int Info()
    {
        Console.WriteLine("[VFS] (MVP) Info view is not implemented yet.");
        return 0;
    }

    private static int Unknown(string cmd)
    {
        Console.WriteLine($"Unknown vfs command: {cmd}");
        return 1;
    }
}
