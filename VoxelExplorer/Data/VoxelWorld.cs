using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Horizon.GameEntity;
using Horizon.OpenGL.Buffers;


namespace VoxelExplorer.Data;

internal class VoxelWorld
{
    public const int WIDTH = 4;
    public const int DEPTH = 4;

    internal Chunk[] Chunks;

    public VoxelWorld()
    {
        Chunks = new Chunk[WIDTH * DEPTH];
        for (int i = 0; i < Chunks.Length; i++)
        {
            Chunks[i] = new Chunk();
        }
    }
}
