using System.Numerics;

namespace Game2D;

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
