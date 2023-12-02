using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using AutoVoxel.World;

using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.World;

using Horizon.Core.Data;

using Silk.NET.OpenGL;

namespace AutoVoxel.Data;

public enum UVCoordinate
{
    TopLeft = 0,
    BottomLeft = 1,
    TopRight = 2,
    BottomRight = 3
}
[StructLayout(LayoutKind.Sequential)]
public struct ChunkVertex
{
    [VertexLayout(0, VertexAttribPointerType.UnsignedInt)]
    private uint packedData; // 4 bytes

    public ChunkVertex(
        int x,
        int y,
        int z,
        CubeFace normal,
        UVCoordinate uv
    )
    {
        packedData =
              (uint)((x & 0b11111) // 0 - 4 = x
            | (y & 0b11111) << 5 // 5 - 9 = y
            | (z & 0b11111) << 10 // 10 - 14 = z
            | ((int)normal & 0b1111) << 14 // 16 - 19 = normal 
            | ((int)uv & 0b111) << 19); // 20 - 22 = texture coordinate 
    }
}
