using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoVoxel.World;

namespace AutoVoxel.Data.Chunks;

internal class LegacySliceData : ISliceData
{
    public Tile[] Tiles = new Tile[Slice.WIDTH * Slice.HEIGHT * Slice.DEPTH];

    public LegacySliceData()
    {
        Array.Fill(Tiles, Tile.Empty);
    }

    public Tile this[int index]
    {
        get =>
            index >= Tiles.Length
                ? Tile.OOB
                : index < 0
                    ? Tile.OOB
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
            int index = x + Slice.WIDTH * (y + Slice.HEIGHT * z);

            return index > -1 && index < Tiles.Length ? Tiles[index]
                : Tile.OOB;
        }
        set
        {
            int index = x + Slice.WIDTH * (y + Slice.HEIGHT * z);

            if (index > -1 && index < Tiles.Length)
                Tiles[index] = value;
        }
    }
}
