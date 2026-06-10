namespace AIKernel.CLI.Commands;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.ClockCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.ClockCommand']" />
public static class ClockCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.ClockCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.ClockCommand.Run']" />
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
        var now = DateTimeOffset.UtcNow;
        Console.WriteLine($"kernel_clock.utc: {now:o}");
        Console.WriteLine($"kernel_clock.unix_ms: {now.ToUnixTimeMilliseconds()}");
        return 0;
    }

    private static int Timeline()
    {
        var now = DateTimeOffset.UtcNow;
        Console.WriteLine("timeline[0].event: kernel.clock.inspect");
        Console.WriteLine($"timeline[0].observed_utc: {now:o}");
        Console.WriteLine($"timeline[0].unix_ms: {now.ToUnixTimeMilliseconds()}");
        return 0;
    }

    private static int Unknown(string cmd)
    {
        Console.WriteLine($"Unknown clock command: {cmd}");
        return 1;
    }
}
