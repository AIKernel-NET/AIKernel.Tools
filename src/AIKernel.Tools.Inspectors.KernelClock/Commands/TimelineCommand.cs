namespace AIKernel.Tools.Inspectors.KernelClock.Commands;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.KernelClock.Commands.TimelineCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.KernelClock.Commands.TimelineCommand']" />
public static class TimelineCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.KernelClock.Commands.TimelineCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.KernelClock.Commands.TimelineCommand.Run']" />
    public static void Run()
    {
        var now = DateTimeOffset.UtcNow;
        Console.WriteLine("timeline[0].event: kernel.clock.inspect");
        Console.WriteLine($"timeline[0].observed_utc: {now:o}");
        Console.WriteLine($"timeline[0].unix_ms: {now.ToUnixTimeMilliseconds()}");
    }
}
