using System.Diagnostics;
using System.Numerics;

namespace TileBash;

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
        RenderingData.TextureID = Map.IsEmpty((int)GlobalPosition.X, (int)GlobalPosition.Y + 1)
            ? TileTextureID.Grass
            : TileTextureID.Dirt;
        if (RenderingData.TextureID == TileTextureID.Grass)
        {
            Console.WriteLine((int)GlobalPosition.Y + 1);
        }
        Chunk.MarkDirty();
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
