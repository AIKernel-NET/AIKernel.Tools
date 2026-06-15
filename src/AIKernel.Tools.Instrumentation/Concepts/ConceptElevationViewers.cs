namespace AIKernel.Tools.Instrumentation.Concepts;

/// <summary>
/// [Canonical layer - Nomos / ノモス]
/// [EN] Upper-level viewer concept for ROM, Canon, and rule-set inspection.
/// [JA] ROM / Canon / rule-set inspection のための上位 viewer concept です。
/// Old technical name: RomViewer / CanonInspector.
/// Do not use this term for DTO, parser, serializer, converter, or low-level CLI infrastructure names.
/// </summary>
public sealed class NomosViewer
{
    /// <summary>
    /// [EN] Creates a stable viewer command alias.
    /// [JA] 安定した viewer command alias を作成します。
    /// </summary>
    public string Alias(string target = "rom")
        => $"aik {target} view";
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Upper-level viewer concept for replay timelines and temporal inspection.
/// [JA] replay timeline と temporal inspection のための上位 viewer concept です。
/// Old technical name: ReplayTimelineViewer.
/// Do not use this term for DTO, parser, serializer, converter, or low-level CLI infrastructure names.
/// </summary>
public sealed class ChronosViewer
{
    /// <summary>
    /// [EN] Creates a stable timeline command alias.
    /// [JA] 安定した timeline command alias を作成します。
    /// </summary>
    public string Alias(string target = "replay")
        => $"aik {target} timeline";
}

/// <summary>
/// [Perception layer - Phantasia / ファンタシア]
/// [EN] Upper-level viewer concept for scene and representation inspection.
/// [JA] scene / representation inspection のための上位 viewer concept です。
/// Old technical name: SceneViewer.
/// Do not use this term for DTO, parser, serializer, converter, or low-level CLI infrastructure names.
/// </summary>
public sealed class PhantasiaViewer
{
    /// <summary>
    /// [EN] Creates a stable scene command alias.
    /// [JA] 安定した scene command alias を作成します。
    /// </summary>
    public string Alias(string target = "scene")
        => $"aik {target} view";
}
