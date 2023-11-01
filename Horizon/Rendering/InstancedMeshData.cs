namespace Horizon.Rendering;

public readonly struct InstancedMeshData<VertexType, InstanceType> : IMeshData<VertexType>
    where VertexType : unmanaged
    where InstanceType : unmanaged
{
    public readonly ReadOnlyMemory<VertexType> Vertices { get; init; }
    public readonly ReadOnlyMemory<InstanceType> InstanceData { get; init; }
    public readonly ReadOnlyMemory<uint> Elements { get; init; }
}