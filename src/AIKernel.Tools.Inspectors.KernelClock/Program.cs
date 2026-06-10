using AIKernel.Tools.Inspectors.KernelClock.Commands;

if (args.Length == 0)
{
    Console.WriteLine("Usage: clock <now|timeline>");
    return;
}

switch (args[0])
{
    case "now":
        NowCommand.Run();
        break;

    case "timeline":
        TimelineCommand.Run();
        break;

    default:
        Console.WriteLine($"Unknown command: {args[0]}");
        break;
}
