using Horizon.Data;

namespace Horizon.Rendering;

public readonly struct MeshData
{
    public required ReadOnlyMemory<Vertex> Vertices { get; init; }
    public required ReadOnlyMemory<uint> Elements { get; init; }
}
