using Horizon.Data;
using Horizon.Rendering.Spriting.Data;

namespace Horizon.Rendering;

public readonly struct MeshData : IMeshData<Vertex>
{
    public readonly ReadOnlyMemory<Vertex> Vertices { get; init; }
    public readonly ReadOnlyMemory<uint> Elements { get; init; }
}
public readonly struct MeshData2D : IMeshData<Vertex2D>
{
    public readonly ReadOnlyMemory<Vertex2D> Vertices { get; init; }
    public readonly ReadOnlyMemory<uint> Elements { get; init; }
}
