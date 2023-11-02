namespace Horizon.Rendering;

/// <summary>
/// Interface allowing mesh delegates using various backends.
/// </summary>
public interface IMeshData<T>
    where T : unmanaged
{
    public ReadOnlyMemory<T> Vertices { get; init; }
    public ReadOnlyMemory<uint> Elements { get; init; }
}
