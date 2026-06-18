namespace AIKernel.Tools.Tests;

using System.Text.RegularExpressions;
using AIKernel.Tools.Instrumentation.Concepts;

/// <summary>
/// EN: Verifies concept vocabulary is limited to upper-level Tool viewers and inspectors.
/// JA: 概念語彙が Tools の上位 viewer / inspector に限定されることを検証します。
/// </summary>
public sealed class ConceptElevationArchitectureTests
{
    private static readonly string[] PhilosophicalPrefixes =
    [
        "Ethos",
        "Pathos",
        "Logos",
        "Nomos",
        "Dike",
        "Kratos",
        "Aisthesis",
        "Phantasia",
        "Chronos",
        "Kairos",
        "Dynamis",
        "Energeia",
        "Nous",
        "Telos",
        "Apatheia",
        "Ataraxia",
        "Eidos",
    ];

    private static readonly string[] ForbiddenTechnicalSuffixes =
    [
        "Dto",
        "Request",
        "Result",
        "Mapper",
        "Adapter",
        "Serializer",
        "Converter",
        "HttpClient",
        "JSInterop",
        "JsInterop",
        "NativeBridge",
        "Provider",
    ];

    private static readonly Regex TypeDeclarationPattern = new(
        @"\b(?:public|internal|private|protected)?\s*(?:sealed\s+|abstract\s+|static\s+|partial\s+)*\b(?:class|record|interface|enum)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    /// <summary>
    /// EN: Confirms Tool concept viewers expose aliases without renaming CLI infrastructure.
    /// JA: CLI infrastructure を rename せず Tool concept viewer が alias を公開することを確認します。
    /// </summary>
    [Fact]
    public void ConceptViewers_WhenUsed_ReturnStableAliases()
    {
        var nomos = new NomosViewer();
        var chronos = new ChronosViewer();
        var phantasia = new PhantasiaViewer();

        Assert.Equal("aik rom view", nomos.Alias());
        Assert.Equal("aik replay timeline", chronos.Alias());
        Assert.Equal("aik scene view", phantasia.Alias());
    }

    /// <summary>
    /// EN: Rejects philosophical prefixes on low-level Tool infrastructure names.
    /// JA: 低レイヤ Tool infrastructure 名への哲学語 prefix を拒否します。
    /// </summary>
    [Fact]
    public void SourceTypes_WhenUsingPhilosophicalPrefix_DoNotUseForbiddenTechnicalSuffix()
    {
        var violations = FindViolations("AIKernel.Tools.slnx");

        Assert.Empty(violations);
    }

    private static IReadOnlyList<string> FindViolations(string solutionFileName)
    {
        var repositoryRoot = FindRepositoryRoot(solutionFileName);
        var sourceRoot = Path.Combine(repositoryRoot, "src");

        return Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => FindViolationsInFile(repositoryRoot, path))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> FindViolationsInFile(string repositoryRoot, string path)
    {
        var source = File.ReadAllText(path);
        foreach (Match match in TypeDeclarationPattern.Matches(source))
        {
            var typeName = match.Groups["name"].Value;
            var hasPhilosophicalPrefix = PhilosophicalPrefixes.Any(prefix => typeName.StartsWith(prefix, StringComparison.Ordinal));
            var hasForbiddenTechnicalSuffix = ForbiddenTechnicalSuffixes.Any(suffix => typeName.EndsWith(suffix, StringComparison.Ordinal));
            var isConceptSurface = path.Contains($"{Path.DirectorySeparatorChar}Concepts{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

            if (hasPhilosophicalPrefix && (hasForbiddenTechnicalSuffix || !isConceptSurface))
            {
                yield return $"{Path.GetRelativePath(repositoryRoot, path)}: {typeName}";
            }
        }
    }

    private static string FindRepositoryRoot(string solutionFileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, solutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate {solutionFileName}.");
    }
}
