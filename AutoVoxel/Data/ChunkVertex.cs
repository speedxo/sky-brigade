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
    private uint packedData0; // 4 bytes
    [VertexLayout(1, VertexAttribPointerType.UnsignedInt)]
    private uint packedData1; // 4 byte

    public readonly Vector3 Position
    {
        get
        {
            Vector3 result;

            result.X = (packedData0 & 0x1F); // 0 - 4 = x
            result.Y = ((packedData0 >> 5) & 0x1F); // 5 - 9 = y
            result.Z = ((packedData0 >> 10) & 0x1F); // 10 - 14 = z

            return result;
        }
    }

    public ChunkVertex(
        int x,
        int y,
        int z,
        CubeFace normal,
        UVCoordinate uv,
        int c = 255,
        TileID id = TileID.Dirt
    )
    {
        /* We use vertex packing to compress vertex data.
         * Each tile locally is within a 32^3 coordinate space, which only needs 5 bytes,
         * the chunks coordinates can be sent via a shader uniform and the final vertex position
         * can be computed in the vertex shader. */
        packedData0 =
              (uint)(
              (x & 0b111111) << 0 // 0 - 5 = x
            | (y & 0b11111111) << 6 // 6 - 13 = y
            | (z & 0b111111) << 14 // 14 - 19 = z
            | ((int)normal & 0b11111) << 20 // 20 - 24 = normal 
            | ((int)uv & 0b11) << 25); // 25 - 27 = texture coordinate

        packedData1 = (uint)(
            // subtract 2 from ID as first 2 IDs are null and air tiles
            (((byte)id - 2) & 0b11111111) << 0 // 0 - 7 = tile id
            );
    }
}
