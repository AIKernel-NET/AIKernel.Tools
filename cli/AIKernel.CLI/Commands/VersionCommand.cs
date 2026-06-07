namespace AIKernel.CLI.Commands;

/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.VersionCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.VersionCommand']" />
public static class VersionCommand
{
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.VersionCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.VersionCommand.Run']" />
    public static int Run(string[] args)
    {
        Console.WriteLine("aik version 0.1.0");
        return 0;
    }
}
