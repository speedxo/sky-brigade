namespace AutoVoxel.Data.Chunks;

internal class JaggedChunkData : ISliceData
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
            //if (x >= Chunk.SIZE || y >= Chunk.SIZE || z >= Chunk.SIZE || x < 0 || y < 0 || z < 0) return Tile.Empty;

            return Tiles[x, y, z];
        }
        set
        {
            //if (x >= Chunk.SIZE || y >= Chunk.SIZE || z >= Chunk.SIZE || x < 0 || y < 0 || z < 0) return;

            Tiles[x, y, z] = value;
        }
    }
}
