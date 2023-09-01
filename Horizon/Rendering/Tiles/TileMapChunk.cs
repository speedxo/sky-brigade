using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Primitives;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileMapChunk : IDrawable, IUpdateable
    {
        public Body? Body { get; set; }

        public const int Width = 32;
        public const int Height = 32;

        public Tile?[,] Tiles { get; init; }

        public Dictionary<TileSet, Tile[,]> TileSetPairs { get; init; }

        public Vector2 Position { get; init; }
        public TileMap Map { get; init; }

        public TilemapRenderer Renderer { get; init; }
        public bool ShouldUpdate { get; set; }

        public RectangleF Bounds { get; init; }
        public bool IsVisibleByCamera { get; set; } = true;

        public TileMapChunk(TileMap map, Vector2 pos)
        {
            Map = map;
            Position = pos;

            if (map.World is not null)
                Body = map.World.CreateBody(new BodyDef {
                    type = BodyType.Static
                });

            Tiles = new Tile?[Width, Height];
            TileSetPairs = new();
            Renderer = new(this);
            ShouldUpdate = true;

            Bounds = new RectangleF(pos * new Vector2(Width - 1, Height - 1) - new Vector2(Tile.TILE_WIDTH / 2.0f, Tile.TILE_HEIGHT / 2.0f), new(Width - 1, Height - 1));
        }

        public bool IsEmpty(int x, int y)
        {
            return this[x, y] == null;
        }

        public Tile? this[int x, int y]
        {
            get
            {
                if (x / Width >= Width || x < 0 || y < 0 || y / Height >= Height)
                    return null;

                return Tiles[x, y];
            }
        }

        public void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (ShouldUpdate)
                GenerateMesh();

            Renderer.Draw(dt, renderOptions);
        }

        public bool FlagForMeshRegeneration()
        {
            if (ShouldUpdate)
                return false;

            ShouldUpdate = true;
            return true;
        }

        public void Update(float dt)
        {
            if (!IsVisibleByCamera) return;

            // todo: stuff
        }

        public void GenerateMesh()
        {
            UpdateTileSetPairs();
            Renderer.GenerateMeshes();
            ShouldUpdate = false;
        }


        private void UpdateTileSetPairs()
        {
            TileSetPairs.Clear();
            var tempPairs = new Dictionary<TileSet, Tile[,]>();

            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile == null) continue;

                    if (!tempPairs.ContainsKey(tile.Set))
                        tempPairs[tile.Set] = new Tile[Width, Height];

                    tempPairs[tile.Set][x, y] = tile;
                }
            }

            foreach ((TileSet set, Tile[,] tiles) in tempPairs)
            {
                TileSetPairs[set] = tiles;
            }

            tempPairs.Clear();
        }

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

        public void Populate(Action<Tile?[,], TileMapChunk> action)
        {
            action(Tiles, this);
        }

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