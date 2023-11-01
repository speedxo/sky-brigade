using Horizon.Data;

namespace Horizon.Rendering;

public readonly struct MeshData : IMeshData<Vertex>
{
    public readonly ReadOnlyMemory<Vertex> Vertices { get; init; }
    public readonly ReadOnlyMemory<uint> Elements { get; init; }
}
