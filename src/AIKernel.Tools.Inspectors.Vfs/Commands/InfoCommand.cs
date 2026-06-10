namespace AIKernel.Tools.Inspectors.Vfs.Commands;

using AIKernel.Common.Results;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.Vfs.Commands.InfoCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.Tools.Inspectors.Vfs.Commands.InfoCommand']" />
public static class InfoCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.Vfs.Commands.InfoCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.Tools.Inspectors.Vfs.Commands.InfoCommand.Run']" />
    public static void Run()
    {
        Run(".");
    }

    /// <summary>
    /// [EN] Prints read-only metadata for a file-system path.
    /// [JA] file-system path の read-only metadata を出力します。
    /// </summary>
    /// <param name="path">
    /// [EN] File or directory path to inspect.
    /// [JA] inspect 対象の file または directory path です。
    /// </param>
    public static void Run(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            var file = new FileInfo(fullPath);
            Console.WriteLine($"path: {file.FullName}");
            Console.WriteLine("type: file");
            Console.WriteLine($"size: {file.Length}");
            Console.WriteLine($"modified_utc: {file.LastWriteTimeUtc:o}");
            return;
        }

        if (Directory.Exists(fullPath))
        {
            var directory = new DirectoryInfo(fullPath);
            Console.WriteLine($"path: {directory.FullName}");
            Console.WriteLine("type: directory");
            Console.WriteLine($"entries: {SafeEnumerateEntries(directory.FullName).Count()}");
            Console.WriteLine($"modified_utc: {directory.LastWriteTimeUtc:o}");
            return;
        }

        Console.WriteLine($"VFS path was not found: {fullPath}");
    }

    private static IEnumerable<string> SafeEnumerateEntries(string root)
    {
        return Try
            .Run(() => Directory.EnumerateFileSystemEntries(root, "*", SearchOption.TopDirectoryOnly).ToArray())
            .Match(_ => Array.Empty<string>(), entries => entries);
    }
}
