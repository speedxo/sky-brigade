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

    //public Tile this[int index]
    //{
    //    get =>
    //        index >= Tiles.Length
    //            ? Tile.Empty
    //            : index < 0
    //                ? Tile.Empty
    //                : Tiles[index];
    //    set
    //    {
    //        if (index < Tiles.Length)
    //            Tiles[index] = value;
    //    }
    //}
    public Tile this[int index]
    {
        get => Tiles[index];
        set => Tiles[index] = value;
    }
    public Tile this[int x, int y, int z]
    {
        get
        {
            int index = x + (Chunk.WIDTH - 1) * (y + (Chunk.HEIGHT - 1) * z);

            //if (index > -1 && index < Tiles.Length)
            return Tiles[index];
            //else return Tile.Empty;

            return index > -1 && index < Tiles.Length ? Tiles[index]
                : Tile.Empty;
        }
        set
        {
            int index = x + (Chunk.WIDTH - 1) * (y + (Chunk.HEIGHT - 1) * z);

            //if (index > -1 && index < Tiles.Length)
            Tiles[index] = value;
        }
    }
}
