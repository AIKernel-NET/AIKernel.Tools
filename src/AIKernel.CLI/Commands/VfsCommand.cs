namespace AIKernel.CLI.Commands;

using AIKernel.Common.Results;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.VfsCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.VfsCommand']" />
public static class VfsCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.VfsCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.VfsCommand.Run']" />
    public static int Run(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: aik vfs <tree|info> [path]");
            return 1;
        }

        return args[0] switch
        {
            "tree" => Tree(PathArgument(args)),
            "info" => Info(PathArgument(args)),
            _ => Unknown(args[0])
        };
    }

    private static int Tree(string path)
    {
        var root = Path.GetFullPath(path);
        if (!Directory.Exists(root))
        {
            Console.WriteLine($"VFS tree root was not found: {root}");
            return 1;
        }

        Console.WriteLine(root);
        foreach (var entry in EnumerateTree(root, maxDepth: 2))
        {
            Console.WriteLine(entry);
        }

        return 0;
    }

    private static int Info(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            var file = new FileInfo(fullPath);
            Console.WriteLine($"path: {file.FullName}");
            Console.WriteLine("type: file");
            Console.WriteLine($"size: {file.Length}");
            Console.WriteLine($"modified_utc: {file.LastWriteTimeUtc:o}");
            return 0;
        }

        if (Directory.Exists(fullPath))
        {
            var directory = new DirectoryInfo(fullPath);
            Console.WriteLine($"path: {directory.FullName}");
            Console.WriteLine("type: directory");
            Console.WriteLine($"entries: {SafeEnumerateEntries(directory.FullName, SearchOption.TopDirectoryOnly).Count()}");
            Console.WriteLine($"modified_utc: {directory.LastWriteTimeUtc:o}");
            return 0;
        }

        Console.WriteLine($"VFS path was not found: {fullPath}");
        return 1;
    }

    private static IEnumerable<string> EnumerateTree(string root, int maxDepth)
    {
        foreach (var entry in SafeEnumerateEntries(root, SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, entry);
            var depth = relative.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar) + 1;
            if (depth > maxDepth)
            {
                continue;
            }

            var prefix = EntryPrefix(entry);
            yield return $"{prefix} {relative}";
        }
    }

    private static string PathArgument(string[] args)
        => ArgumentOption(args, 1).Match(() => ".", value => value);

    private static Option<string> ArgumentOption(string[] args, int index)
        => args.Length > index
            ? Option<string>.Some(args[index])
            : Option<string>.None();

    private static string EntryPrefix(string entry)
        => MonadicDecision.SelectText(Directory.Exists(entry), "[F]", "[D]");

    private static IEnumerable<string> SafeEnumerateEntries(string root, SearchOption searchOption)
        => Try.Run(() => Directory.EnumerateFileSystemEntries(root, "*", searchOption).ToArray())
            .Match(_ => Array.Empty<string>(), entries => entries);

    private static int Unknown(string cmd)
    {
        Console.WriteLine($"Unknown vfs command: {cmd}");
        return 1;
    }
}
