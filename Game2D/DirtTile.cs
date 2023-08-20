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
    }

    public override void PostGeneration(int x, int y)
    {
        if (y == 0) RenderingData.TextureID = GameScene.TileTextureID.Grass;
        else RenderingData.TextureID = GameScene.TileTextureID.Dirt;

        ShouldUpdateMesh = true;
    }
}

