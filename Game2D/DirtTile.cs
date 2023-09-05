using System.Numerics;

namespace Game2D;

public class DirtTile : Tile
{
    public DirtTile(TileMapChunk chunk, Vector2 local)
        : base(chunk, local)
    {
        RenderingData.TextureID = TileTextureID.Dirt;
        Set = Map.GetTileSetFromTileTextureID(RenderingData.TextureID);
        PhysicsData.IsCollidable = false;
    }
    public override void PostGeneration()
    {
        RenderingData.TextureID = Map.IsEmpty((int)GlobalPosition.X, (int)GlobalPosition.Y + 1) ?
            TileTextureID.Grass
            : TileTextureID.Dirt;
        if (RenderingData.TextureID == TileTextureID.Grass)
        {
            Console.WriteLine((int)GlobalPosition.Y + 1);
        }
        Chunk.MarkDirty();
    }
}

public class CobblestoneTile : Tile
{
    public CobblestoneTile(TileMapChunk chunk, Vector2 local)
        : base(chunk, local)
    {
        RenderingData.TextureID = TileTextureID.Cobblestone;
        Set = Map.GetTileSetFromTileTextureID(RenderingData.TextureID);
        PhysicsData.IsCollidable = false;
    }
}