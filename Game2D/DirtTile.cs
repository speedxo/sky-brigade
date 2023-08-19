using System.Numerics;
using Horizon.Rendering;
using Horizon.Rendering.Tilemap;

namespace Game2D;

public class DirtTile : Tile
{
    public DirtTile(Tilemap map, Vector2 local, Vector2 global)
        : base(map, local, global)
    {
        Sheet = map.Sheets["tileset"];
    }

    public override void PostGeneration(int x, int y)
    {
        if (y == 0) RenderingData = RenderingData with { FrameName = "grass" };
        else RenderingData = RenderingData with { FrameName = "dirt" };
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {

    }

    public override void Update(float dt)
    {

    }
}

