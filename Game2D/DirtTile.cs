using System.Numerics;
using Horizon.Rendering;
using static Horizon.Rendering.Tiling<Game2D.GameScene.TileID, Game2D.GameScene.TileTextureID>;

namespace Game2D;

public class DirtTile : Tile
{
    public DirtTile(TileMapChunk chunk, Vector2 local)
        : base(chunk, local)
    {
        RenderingData.TextureID = GameScene.TileTextureID.Dirt;
        IsCollidable = false;
    }

    public override void PostGeneration(int x, int y)
    {
        RenderingData.TextureID = Chunk.IsEmpty(x, y + 1) ? GameScene.TileTextureID.Grass : GameScene.TileTextureID.Dirt;
        Chunk.FlagForMeshRegeneration();
    }
}

