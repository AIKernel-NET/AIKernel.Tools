namespace AIKernel.Tools.Inspectors.KernelClock.Commands;

/// <summary>[EN] Documents this public package API member. [JA] NowCommand を表します。</summary>
/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand']" />
public static class NowCommand
{
    /// <summary>[EN] Documents this public package API member. [JA] Run を実行します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.KernelClock.Commands.NowCommand.Run']" />
    public static void Run()
    {
        var now = DateTimeOffset.UtcNow;
        Console.WriteLine($"kernel_clock.utc: {now:o}");
        Console.WriteLine($"kernel_clock.unix_ms: {now.ToUnixTimeMilliseconds()}");
    }
}
