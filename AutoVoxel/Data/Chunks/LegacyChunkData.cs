using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoVoxel.World;

namespace AutoVoxel.Data.Chunks;

internal class LegacyChunkData : IChunkData
{
    public Tile[] Tiles = new Tile[Chunk.WIDTH * Chunk.HEIGHT * Chunk.DEPTH];

    public Tile this[int index]
    {
        get =>
            index >= Tiles.Length
                ? Tile.Empty
                : index < 0
                    ? Tile.Empty
                    : Tiles[index];
        set
        {
            if (index < Tiles.Length)
                Tiles[index] = value;
        }
    }

    public Tile this[int x, int y, int z]
    {
        get
        {
            int index = x + (y * Chunk.WIDTH) + (z * Chunk.WIDTH * Chunk.HEIGHT);

            return index >= 0 && index < Tiles.Length ? Tiles[index] : Tile.Empty;
        }
        set
        {
            int index = x + (y * Chunk.WIDTH) + (z * Chunk.WIDTH * Chunk.HEIGHT);

            if (index >= 0 && index < Tiles.Length)
                Tiles[index] = value;
        }
    }
}
