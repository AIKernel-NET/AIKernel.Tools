namespace KernelClockInspector.Commands;

public static class NowCommand
{
    public static void Run()
    {
        Console.WriteLine($"KernelClock (mock): {DateTimeOffset.UtcNow:o}");
    }
}
