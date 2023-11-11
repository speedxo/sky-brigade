using Horizon.Data;
using Horizon.Rendering.Spriting.Data;

namespace Horizon.Rendering;

/// <summary>
/// Supplementary 3D implementation of IMeshData<T> for the <see cref="Vertex"/>
/// </summary>
/// <seealso cref="Horizon.Rendering.IMeshData&lt;Horizon.Data.Vertex&gt;" />
public readonly struct MeshData : IMeshData<Vertex>
{
    public readonly ReadOnlyMemory<Vertex> Vertices { get; init; }
    public readonly ReadOnlyMemory<uint> Elements { get; init; }
}

/// <summary>
/// Supplementary 2D implementation of IMeshData<T> for the <see cref="Vertex2D"/>
/// </summary>
/// <seealso cref="Horizon.Rendering.IMeshData&lt;Horizon.Data.Vertex&gt;" />
public readonly struct MeshData2D : IMeshData<Vertex2D>
{
    public readonly ReadOnlyMemory<Vertex2D> Vertices { get; init; }
    public readonly ReadOnlyMemory<uint> Elements { get; init; }
}
