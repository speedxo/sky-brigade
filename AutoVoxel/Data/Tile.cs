namespace AutoVoxel.Data;

public struct Tile
{
    public TileID ID;

    public static Tile Empty { get; } = new Tile() { ID = TileID.Air };
    public static Tile OOB { get; } = new Tile() { ID = TileID.Null };
}
