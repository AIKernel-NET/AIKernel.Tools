using AIKernel.Tools.Inspectors.Vfs.Commands;
using AIKernel.Common.Results;

if (args.Length == 0)
{
    Console.WriteLine("Usage: vfs <tree|info> [path]");
    return;
}

var path = ArgumentOption(args, 1).Match(() => ".", value => value);

switch (args[0])
{
    case "tree":
        TreeCommand.Run(path);
        break;

    case "info":
        InfoCommand.Run(path);
        break;

    default:
        Console.WriteLine($"Unknown command: {args[0]}");
        break;
}

static Option<string> ArgumentOption(string[] values, int index)
    => values.Length > index
        ? Option<string>.Some(values[index])
        : Option<string>.None();
