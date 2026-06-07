namespace AIKernel.Tools.Inspectors.KernelClock.Commands;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand']" />
public static class NowCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand.Run']" />
    public static void Run()
    {
        Console.WriteLine($"KernelClock (mock): {DateTimeOffset.UtcNow:o}");
    }
}
