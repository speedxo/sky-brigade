using System.Numerics;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering.Tilemap;

public class TileMapChunk
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;

    private static Random rand = new();

    public Tile[,] Tiles { get; init; }
    public Dictionary<Spritesheet, Tile[]> SpritesheetTilePairs { get; init; }

    public int Slice { get; init; }
    public Tilemap Map { get; init; }

    public TilemapRenderer Renderer { get; init; }

    public TileMapChunk(Tilemap map, int slice)
    {
        this.Map = map;
        this.Slice = slice;

        this.Tiles = new Tile[WIDTH, HEIGHT];
        this.SpritesheetTilePairs = new();
        this.Renderer = new(this);
    }

    public void Draw(float dt, RenderOptions? renderOptions = null)
    {
        Renderer.Draw(dt, renderOptions);
    }

    public void GenerateMesh()
    {
        UpdateSpritesheetPairs();
        Renderer.GenerateMeshes(Slice);
    }

    private void UpdateSpritesheetPairs()
    {
        SpritesheetTilePairs.Clear();
        var tempPairs = new Dictionary<Spritesheet, List<Tile>>();

        for (int x = 0; x < Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < Tiles.GetLength(1); y++)
            {
                var tile = Tiles[x, y];

                if (!tempPairs.ContainsKey(tile.Sheet))
                    tempPairs.Add(tile.Sheet, new List<Tile>());

                tempPairs[tile.Sheet].Add(tile);
            }
        }

        foreach ((Spritesheet sheet, List<Tile> tiles) in tempPairs)
        {
            SpritesheetTilePairs[sheet] = tiles.ToArray();
        }

        tempPairs.Clear();
    }

    public void Generate(Func<Vector2, Vector2, Tile> generateTileFunc)
    {
        Parallel.For(0, Tiles.GetLength(0), x =>
        {
            for (int y = 0; y < Tiles.GetLength(1); y++)
            {
                Tiles[x, y] = generateTileFunc(new Vector2(x, y), new Vector2(x + Slice * WIDTH, y));
            }
        });
    }
    public void PostGenerate()
    {
        for (int x = 0; x < Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < Tiles.GetLength(1); y++)
            {
                Tiles[x, y].PostGeneration(x, y);
            }
        }
    }
}

