using System.Numerics;
using Horizon.Primitives;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering.Tilemap;

public abstract class Tile : IUpdateable, IDrawable
{
    public const float TILE_WIDTH = 0.1f;
    public const float TILE_HEIGHT = 0.1f;

    public struct TileRenderingData
    {
        public string FrameName { get; set; }

        public static TileRenderingData Default { get; } = new TileRenderingData
        {
            FrameName = "debug"
        };
    }
    public Vector2 Position { get; private set; }
    public TileRenderingData RenderingData { get; protected set; }
    public Spritesheet Sheet { get; set; }
    public int ID { get; protected set; }
    public Tilemap Map { get; set; }

    public Tile(Tilemap map, Vector2 pos)
    {
        this.Map = map;
        this.Position = pos;
        this.RenderingData = TileRenderingData.Default;
    }

    public abstract void Draw(float dt, RenderOptions? renderOptions = null);
    public abstract void Update(float dt);

    public virtual void PostGeneration(int x, int y)
    {

    }
}

