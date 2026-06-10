using System.Text;
using AIKernel.Common.Results;
using AIKernel.Tools.Instrumentation.Abstractions;

namespace AIKernel.Tools.Instrumentation;

/// <summary>
/// [EN] Deterministic replay engine for line-oriented replay material.
/// [JA] 行指向 replay material 向けの deterministic replay engine です。
/// </summary>
public sealed class ReplayEngine : IReplayEngine
{
    private readonly List<string> _events = [];
    private readonly SortedDictionary<string, string> _metadata = new(StringComparer.Ordinal);
    private int _position;
    private string _state = "Created";

    /// <summary>
    /// [EN] Loads replay material from a UTF-8 text file.
    /// [JA] UTF-8 text file から replay material を読み込みます。
    /// </summary>
    /// <param name="path">
    /// [EN] Replay material path.
    /// [JA] replay material path です。
    /// </param>
    /// <returns>
    /// [EN] This replay engine.
    /// [JA] この replay engine です。
    /// </returns>
    public ReplayEngine Load(string path)
        => (ReplayEngine)RequireSuccess(TryLoad(path));

    /// <inheritdoc />
    public Result<IReplayEngine> TryLoad(string path)
    {
        return
            from validPath in ValidatePath(path)
            from loaded in Try.Run<IReplayEngine>(() =>
            {
                var fullPath = Path.GetFullPath(validPath);
                var events = File.ReadAllLines(fullPath, Encoding.UTF8);
                _events.Clear();
                _events.AddRange(events);
                _metadata.Clear();
                _metadata["path"] = fullPath;
                _metadata["event_count"] = _events.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
                _position = 0;
                _state = "Loaded";
                return this;
            })
            select loaded;
    }

    /// <summary>
    /// [EN] Runs the loaded replay material to completion.
    /// [JA] 読み込まれた replay material を完了まで実行します。
    /// </summary>
    /// <returns>
    /// [EN] Completed replay session snapshot.
    /// [JA] 完了した replay session snapshot です。
    /// </returns>
    public ReplaySession Run()
        => RequireSuccess(TryRun());

    /// <inheritdoc />
    public Result<ReplaySession> TryRun()
    {
        _position = _events.Count;
        _state = "Completed";
        return TrySession();
    }

    /// <summary>
    /// [EN] Advances replay by one event and returns the observed event.
    /// [JA] replay を 1 event 進め、観測された event を返します。
    /// </summary>
    /// <returns>
    /// [EN] The next event, or null when replay is complete.
    /// [JA] 次の event、または replay 完了時は null です。
    /// </returns>
    public string? Step()
    {
        var option = RequireSuccess(TryStep());
        return option.Match<string?>(() => null, value => value);
    }

    /// <inheritdoc />
    public Result<Option<string>> TryStep()
    {
        if (_position >= _events.Count)
        {
            _state = "Completed";
            return Result<Option<string>>.Success(Option<string>.None());
        }

        var next = _events[_position];
        _position++;
        _state = ReplayStateAfterStep(_position, _events.Count);
        return Result<Option<string>>.Success(Option<string>.Some(next));
    }

    /// <summary>
    /// [EN] Returns the current replay session snapshot.
    /// [JA] 現在の replay session snapshot を返します。
    /// </summary>
    /// <returns>
    /// [EN] Current replay session.
    /// [JA] 現在の replay session です。
    /// </returns>
    public ReplaySession Session()
        => RequireSuccess(TrySession());

    /// <inheritdoc />
    public Result<ReplaySession> TrySession()
    {
        return Try.Run(() =>
        {
            var metadata = new SortedDictionary<string, string>(_metadata, StringComparer.Ordinal)
            {
                ["position"] = _position.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            return new ReplaySession(_events, _state, metadata);
        });
    }

    private static Result<string> ValidatePath(string path)
        => string.IsNullOrWhiteSpace(path)
            ? Result<string>.Fail("Replay path is required. ErrorCode=TOOLS_REPLAY_PATH_REQUIRED")
            : Result<string>.Success(path);

    private static string ReplayStateAfterStep(int position, int eventCount)
        => MonadicDecision.SelectText(position >= eventCount, "Running", "Completed");

    private static T RequireSuccess<T>(Result<T> result)
        => result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);
}
