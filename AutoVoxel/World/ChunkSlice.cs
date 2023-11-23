using AutoVoxel.Data;

namespace AutoVoxel.World;

public class ChunkSlice
{
    public const int HEIGHT = 8;

    public Tile[] Tiles { get; private set; }
    public bool IsSingle { get; private set; }
    public Tile SingleType { get; private set; }

    private bool wasSingle = false;

    public void Insert(in Tile tile, in int x, in int z, in int y)
    {
        if (Tiles is null && !wasSingle)
        {
            SingleType = tile;
            IsSingle = true;
            wasSingle = true;
        }

        if (IsSingle && tile.ID == SingleType.ID) return;

        IsSingle = false;
        Tiles = new Tile[Chunk.WIDTH * Chunk.DEPTH * HEIGHT];

        int i = x + Chunk.WIDTH * (y + Chunk.DEPTH * z);
        if (i > Tiles.Length - 1) return;
        Tiles[i] = tile;

    }

    public Tile Get(in int x, in int z, in int y)
    {
        if (IsSingle) 
            return SingleType;

        int i = x + Chunk.WIDTH * (y + Chunk.DEPTH * z);
        if (i > Tiles.Length - 1) return default;

        return Tiles[i];
    }
}
