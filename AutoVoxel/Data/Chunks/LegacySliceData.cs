using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVoxel.Data.Chunks;

/// <summary>
/// (VERY BAD) Slice data structure storing data in a ushort indexed hashset (dictionary)
/// </summary>
internal class HashedSliceData : ISliceData
{
    public Dictionary<ushort, Tile> tiles;

    public HashedSliceData()
    {
        tiles = new();
    }

    public Tile this[int x, int y, int z]
    {
        get
        {
            ushort index = (ushort)(x + y * Slice.SIZE + z * Slice.SIZE * Slice.SIZE);

            if (tiles.TryGetValue(index, out Tile tile))
                return tile;

            return Tile.Empty;
        }
        set
        {
            ushort index = (ushort)(x + y * Slice.SIZE + z * Slice.SIZE * Slice.SIZE);

            tiles.Remove(index);
            tiles.Add(index, value);
        }
    }
}

/// <summary>
/// (ALRIGHT) Slice data structure storing data in a flattened array.
/// </summary>
internal class LegacySliceData : ISliceData
{
    public Tile[] Tiles = new Tile[Slice.SIZE * Slice.SIZE * Slice.SIZE];

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
            int index = x + Slice.SIZE * (y + Slice.SIZE * z);

            return index > -1 && index < Tiles.Length ? Tiles[index]
                : Tile.OOB;
        }
        set
        {
            int index = x + Slice.SIZE * (y + Slice.SIZE * z);

            if (index > -1 && index < Tiles.Length)
                Tiles[index] = value;
        }
    }
}