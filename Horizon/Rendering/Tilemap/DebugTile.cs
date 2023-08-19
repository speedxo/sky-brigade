namespace Horizon.Rendering.Tilemap;

public class DebugTile : Tile
{
    public DebugTile(Tilemap map, System.Numerics.Vector2 pos)
        : base(map, pos)
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

