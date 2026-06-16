namespace AIKernel.Tools.Instrumentation;

/// <summary>
/// [EN] Static convenience facade for canonical serialization.
/// [JA] canonical serialization 向けの static convenience facade です。
/// </summary>
public static class CanonicalSerializer
{
    /// <summary>
    /// [EN] Serializes a value with deterministic formatting semantics.
    /// [JA] deterministic formatting semantics で value を serialize します。
    /// </summary>
    /// <param name="value">
    /// [EN] Value to serialize.
    /// [JA] serialize 対象の value です。
    /// </param>
    /// <returns>
    /// [EN] Canonical serialized text.
    /// [JA] canonical serialized text です。
    /// </returns>
    public static string Serialize(object? value)
        => new CanonicalFormatter().Serialize(value);
}
