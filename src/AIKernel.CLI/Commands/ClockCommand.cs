namespace AIKernel.CLI.Commands;

public static class ClockCommand
{
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik clock <now|timeline>");
            return 1;
        }

        return args[0] switch
        {
            "now" => Now(),
            "timeline" => Timeline(),
            _ => Unknown(args[0])
        };
    }

    private static int Now()
    {
        Console.WriteLine($"KernelClock (mock): {DateTimeOffset.UtcNow:o}");
        return 0;
    }

    private static int Timeline()
    {
        Console.WriteLine("[Clock] (MVP) Timeline view is not implemented yet.");
        return 0;
    }

    private static int Unknown(string cmd)
    {
        Console.WriteLine($"Unknown clock command: {cmd}");
        return 1;
    }
}
