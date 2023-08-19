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

    public Tile this[int x, int y]
    {
        get
        {
            return ChunkManager.Chunks[x / WIDTH].Tiles[x % TileMapChunk.WIDTH, y];
        }
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

    public void GenerateTiles(Func<Vector2, Vector2, Tile> generateTileFunc)
    {
        ChunkManager.GenerateTiles(generateTileFunc);
    }
}

