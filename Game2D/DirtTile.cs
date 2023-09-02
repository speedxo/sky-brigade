using System.Numerics;
using static Horizon.Rendering.Tiling<Game2D.GameScene.TileID, Game2D.GameScene.TileTextureID>;

namespace Game2D;

public class DirtTile : Tile
{
    public DirtTile(TileMapChunk chunk, Vector2 local)
        : base(chunk, local)
    {
        RenderingData.TextureID = GameScene.TileTextureID.Dirt;
        PhysicsData.IsCollidable = false;
    }

    //public override bool TryGenerateCollider()
    //{
    //    RenderingData.Color.X = 0.0f;
    //    RenderingData.Color.Y = 1.0f;
    //    Chunk.MarkDirty();

    //    return base.TryGenerateCollider();
    //}

    //public override bool TryDestroyCollider()
    //{
    //    RenderingData.Color.X = 1.0f;
    //    RenderingData.Color.Y = 0.0f;
    //    Chunk.MarkDirty();

    //    return base.TryDestroyCollider();
    //}

    public override void PostGeneration()
    {
        RenderingData.TextureID = Map.IsEmpty((int)GlobalPosition.X, (int)GlobalPosition.Y + 1) ? GameScene.TileTextureID.Grass : GameScene.TileTextureID.Dirt;
        Chunk.MarkDirty();
    }
}