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

    public Vector3 Position
    {
        get
        {
            Vector3 result;

            result.X = (packedData & 0x1F); // 0 - 4 = x
            result.Y = ((packedData >> 5) & 0x1F); // 5 - 9 = y
            result.Z = ((packedData >> 10) & 0x1F); // 10 - 14 = z

            return result;
        }
    }

    public ChunkVertex(
        int x,
        int y,
        int z,
        CubeFace normal,
        UVCoordinate uv,
        int c = 255
    )
    {
        /* We use vertex packing to compress vertex data.
         * Each tile locally is within a 32^3 coordinate space, which only needs 5 bytes,
         * the chunks coordinates can be sent via a shader uniform and the final vertex position
         * can be computed in the vertex shader. */
        packedData =
              (uint)(
              (x & 0b111111) << 0 // 0 - 5 = x
            | (y & 0b111111) << 6 // 6 - 11 = y
            | (z & 0b111111) << 12 // 12 - 17 = z
            | ((int)normal & 0b11111) << 18 // 18 - 22 = normal 
            | ((int)uv & 0b11) << 23 // 23 - 24 = texture coordinate 
            | (c & 0b1111) << 25); // 25 - 28 = shade
    }
}
