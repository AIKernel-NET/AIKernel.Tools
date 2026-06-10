namespace AIKernel.Tools.Inspectors.Vfs.Commands;

using AIKernel.Common.Results;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.Vfs.Commands.TreeCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.Vfs.Commands.TreeCommand']" />
public static class TreeCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.Vfs.Commands.TreeCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.Vfs.Commands.TreeCommand.Run']" />
    public static void Run()
    {
        Run(".");
    }

    /// <summary>
    /// [EN] Prints a bounded directory tree rooted at the specified path.
    /// [JA] 指定された path を root とする bounded directory tree を出力します。
    /// </summary>
    /// <param name="path">
    /// [EN] Directory path to inspect.
    /// [JA] inspect 対象の directory path です。
    /// </param>
    public static void Run(string path)
    {
        var root = Path.GetFullPath(path);
        if (!Directory.Exists(root))
        {
            Console.WriteLine($"VFS tree root was not found: {root}");
            return;
        }

        Console.WriteLine(root);
        foreach (var entry in EnumerateTree(root, maxDepth: 2))
        {
            Console.WriteLine(entry);
        }
    }

    private static IEnumerable<string> EnumerateTree(string root, int maxDepth)
    {
        foreach (var entry in SafeEnumerateEntries(root).OrderBy(x => x, StringComparer.Ordinal))
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

    private static string EntryPrefix(string entry)
        => MonadicDecision.SelectText(Directory.Exists(entry), "[F]", "[D]");

    private static IEnumerable<string> SafeEnumerateEntries(string root)
    {
        return Try
            .Run(() => Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories).ToArray())
            .Match(_ => Array.Empty<string>(), entries => entries);
    }
}
