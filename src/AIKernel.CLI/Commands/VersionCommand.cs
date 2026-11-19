using System.Reflection;

namespace AIKernel.CLI.Commands;

/// <summary>[EN] Documents this public package API member. [JA] VersionCommand を表します。</summary>
/// <include file="docs.en.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.VersionCommand']" />
/// <include file="docs.ja.xml" path="doc/members/member[@name='T:AIKernel.CLI.Commands.VersionCommand']" />
public static class VersionCommand
{
    /// <summary>[EN] Documents this public package API member. [JA] Run を実行します。</summary>
    /// <include file="docs.en.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.VersionCommand.Run']" />
    /// <include file="docs.ja.xml" path="doc/members/member[@name='M:AIKernel.CLI.Commands.VersionCommand.Run']" />
    public static int Run(string[] args)
    {
        var assembly = typeof(VersionCommand).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0] ?? assembly.GetName().Version!.ToString(3);
        Console.WriteLine($"aik version {version}");
        return 0;
    }
}
