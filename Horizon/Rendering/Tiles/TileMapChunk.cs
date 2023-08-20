using System.Numerics;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileMapChunk
    {
        public const int WIDTH = 32;
        public const int HEIGHT = 32;

        public Tile?[,] Tiles { get; init; }
        public Dictionary<TileSet, Tile[,]> TileTileSetPairs { get; init; }

        public int Slice { get; init; }
        public TileMap Map { get; init; }

        public TilemapRenderer Renderer { get; init; }

        public TileMapChunk(TileMap map, int slice)
        {
            this.Map = map;
            this.Slice = slice;

            this.Tiles = new Tile?[WIDTH, HEIGHT];
            this.TileTileSetPairs = new();
            this.Renderer = new(this);
        }

        public bool IsEmpty(int x, int y)
        {
            return this[x, y] == null;
        }

        public Tile? this[int x, int y]
        {
            get
            {
                if (x / WIDTH > WIDTH - 1) return null;
                if (x < 0 || y < 0 || x > WIDTH - 1 || y > HEIGHT - 1) return null;

                return Tiles[x, y];
            }
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
            TileTileSetPairs.Clear();
            var tempPairs = new Dictionary<TileSet, Tile[,]>();

            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile == null) continue;

                    if (!tempPairs.ContainsKey(tile.Set))
                        tempPairs.Add(tile.Set, new Tile[WIDTH, HEIGHT]);

                    tempPairs[tile.Set][x, y] = tile;
                }
            }

            foreach ((TileSet set, Tile[,] tiles) in tempPairs)
            {
                TileTileSetPairs[set] = tiles;
            }

            tempPairs.Clear();
        }

        /// <summary>
        /// This method accepts a generator function that is called for each position in the tilemap.
        /// </summary>
        /// <param name="generateTileFunc">The generator function</param>
        public void Generate(Func<TileMapChunk, Vector2, Tile?> generateTileFunc)
        {
            Parallel.For(0, Tiles.GetLength(0), x =>
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    Tiles[x, y] = generateTileFunc(this, new Vector2(x, y));
                }
            });
        }

        /// <summary>
        /// This method accepts a populator action that is expected to fill the tile[].
        /// </summary>
        /// <param name="action">The populator action</param>
        public void Populate(Action<Tile?[,], TileMapChunk> action)
            => action(Tiles, this);

        public void PostGenerate()
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    Tiles[x, y]?.PostGeneration(x, y);
                }
            }
        }
    }

}