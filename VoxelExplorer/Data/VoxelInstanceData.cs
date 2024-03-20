using System.Runtime.InteropServices;

using Horizon.Core.Data;

using Silk.NET.OpenGL;


namespace VoxelExplorer.Data;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct VoxelInstanceData(int packedData)
{
    [VertexLayout(0, VertexAttribPointerType.Int)]
    private readonly int PackedData = packedData;

    public static VoxelInstanceData Encode(in int x, in int y, in int z, in int face)
    {
        return new VoxelInstanceData((0b1111 & x) << 0 | (0b1111 & y) << 4 | (0b1111 & z) << 8 | (0b111 & face) << 12);
    }
}
