using System.Numerics;
using Horizon.Primitives;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public partial class Tiling<TTileID, TTextureID>
    where TTileID : Enum
    where TTextureID : Enum
{
    public abstract class Tile : IUpdateable, IDrawable
    {
        public const float TILE_WIDTH = 0.1f;
        public const float TILE_HEIGHT = 0.1f;

        public Vector2 LocalPosition { get; protected set; }
        public Vector2 GlobalPosition { get; protected set; }
        public TTileID ID { get; protected set; }

        public bool ShouldUpdateMesh { get; set; }
        public TileRenderingData RenderingData { get; init; }
        public TileMapChunk Chunk { get; init; }
        public TileMap Map => Chunk.Map;

        public TileSet Set => Map.GetTileSetFromTileTextureID(RenderingData.TextureID);

        public Tile(TileMapChunk chunk, Vector2 local)
        {
            this.Chunk = chunk;
            this.LocalPosition = local;
            this.GlobalPosition = local + new Vector2(chunk.Slice * TileMapChunk.WIDTH, 0);
            this.RenderingData = new();
        }

        public virtual void Draw(float dt, RenderOptions? renderOptions = null)
        {

        }
        public virtual void Update(float dt)
        {

        }

        public virtual void PostGeneration(int x, int y)
        {

        }
    }
}

