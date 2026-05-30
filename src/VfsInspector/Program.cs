using VfsInspector.Commands;

if (args.Length == 0)
{
    Console.WriteLine("Usage: vfs <tree|info>");
    return;
}

switch (args[0])
{
    case "tree":
        TreeCommand.Run();
        break;

    case "info":
        InfoCommand.Run();
        break;

    default:
        Console.WriteLine($"Unknown command: {args[0]}");
        break;
}
