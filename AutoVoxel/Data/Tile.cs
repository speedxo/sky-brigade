namespace AutoVoxel.Data;

public struct Tile
{
    public TileID ID;

    public static Tile Empty { get; } = new Tile() { ID = 0 };
}
