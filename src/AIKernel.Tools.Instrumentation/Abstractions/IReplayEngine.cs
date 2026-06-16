using AIKernel.Common.Results;

namespace AIKernel.Tools.Instrumentation.Abstractions;

/// <summary>
/// [EN] Stable replay engine contract for deterministic line-oriented replay material.
/// [JA] deterministic な行指向 replay material 向けの安定した replay engine contract です。
/// </summary>
public interface IReplayEngine
{
    /// <summary>
    /// [EN] Safely loads replay material.
    /// [JA] replay material を安全に load します。
    /// </summary>
    /// <param name="path">
    /// [EN] Replay material path.
    /// [JA] replay material path です。
    /// </param>
    /// <returns>
    /// [EN] Loaded replay engine result.
    /// [JA] load 済み replay engine result です。
    /// </returns>
    Result<IReplayEngine> TryLoad(string path);

    /// <summary>
    /// [EN] Safely runs the loaded replay material.
    /// [JA] load 済み replay material を安全に実行します。
    /// </summary>
    /// <returns>
    /// [EN] Replay session result.
    /// [JA] replay session result です。
    /// </returns>
    Result<ReplaySession> TryRun();

    /// <summary>
    /// [EN] Safely advances replay by one step.
    /// [JA] replay を 1 step 安全に進めます。
    /// </summary>
    /// <returns>
    /// [EN] Optional replay event.
    /// [JA] 任意の replay event です。
    /// </returns>
    Result<Option<string>> TryStep();

    /// <summary>
    /// [EN] Safely returns the current replay session.
    /// [JA] 現在の replay session を安全に返します。
    /// </summary>
    /// <returns>
    /// [EN] Replay session result.
    /// [JA] replay session result です。
    /// </returns>
    Result<ReplaySession> TrySession();
}
