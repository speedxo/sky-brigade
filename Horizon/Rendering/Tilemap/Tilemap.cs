using System.Numerics;
using Horizon.GameEntity;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering.Tilemap;

public class Tilemap : Entity
{
    public const int WIDTH = 4;

    public TilemapChunkManager ChunkManager { get; init; }

    public Dictionary<string, Spritesheet> Sheets { get; init; }

    public Tilemap()
    {
        Name = "Tilemap";

        Sheets = new() {
            { "default", new Spritesheet(GameManager.Instance.ContentManager.GetTexture("debug"), new Vector2(273, 360)) }
        };
        Sheets["default"].AddSprite("debug", Vector2.Zero);

        ChunkManager = AddComponent<TilemapChunkManager>();
    }

    public Spritesheet AddSpritesheet(string name, Spritesheet sheet)
    {
        Sheets.Add(name, sheet);
        return sheet;
    }

    public void GenerateMeshes()
    {
        ChunkManager.GenerateMeshes();
    }

    public void GenerateTiles(Func<int, int, Tile> generateTileFunc)
    {
        ChunkManager.GenerateTiles(generateTileFunc);
    }
}

