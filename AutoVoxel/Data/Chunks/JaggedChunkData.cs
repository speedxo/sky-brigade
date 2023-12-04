using AutoVoxel.World;

namespace AutoVoxel.Data.Chunks;

internal class JaggedChunkData : IChunkData
{
    public Tile[,,] Tiles = new Tile[Chunk.WIDTH, Chunk.HEIGHT, Chunk.DEPTH];

    public Tile this[int index]
    {
        get => Tile.Empty;
        set => _ = value;
    }

    public Tile this[int x, int y, int z]
    {
        get
        {
            //if (x >= Chunk.WIDTH || y >= Chunk.HEIGHT || z >= Chunk.DEPTH || x < 0 || y < 0 || z < 0) return Tile.Empty;

            return Tiles[x, y, z];
        }
        set
        {
            //if (x >= Chunk.WIDTH || y >= Chunk.HEIGHT || z >= Chunk.DEPTH || x < 0 || y < 0 || z < 0) return;

            Tiles[x, y, z] = value;
        }
    }
}
