namespace Horizon.Core.Primitives;

/// <summary>
/// The most primitive type, represents any handle bound object.
/// </summary>
public interface IGLObject
{
    public uint Handle { get; init; }
}
