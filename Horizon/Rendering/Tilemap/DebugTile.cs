using System.Numerics;

namespace Horizon.Rendering.Tilemap;

public class DebugTile : Tile
{
    public DebugTile(Tilemap map, Vector2 local, Vector2 global)
        : base(map, local, global)
    {
        ID = -1;
        Sheet = map.Sheets["default"];
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {

    }

    public override void Update(float dt)
    {

    }
}

