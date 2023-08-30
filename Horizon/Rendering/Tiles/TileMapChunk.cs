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

        public int Slice { get; init; }
        public TileMap Map { get; init; }

        public TilemapRenderer Renderer { get; init; }
        public bool ShouldUpdate { get; set; }

        public TileMapChunk(TileMap map, int slice)
        {
            Map = map;
            Slice = slice;

            if (map.World is not null)
                Body = map.World.CreateBody(new BodyDef {
                type = BodyType.Static
            });

            Tiles = new Tile?[Width, Height];
            TileSetPairs = new();
            Renderer = new(this);
            ShouldUpdate = true;
        }

        public bool IsEmpty(int x, int y)
        {
            return this[x, y] == null;
        }

        public Tile? this[int x, int y]
        {
            get
            {
                if (x / Width >= Width || x < 0 || y < 0 || y >= Height)
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

        public void Update(float dt)
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile is null) continue;

                    if (tile.ShouldUpdateMesh)
                    {
                        ShouldUpdate = true;
                        return;
                    }
                }
            }
        }

        public void GenerateMesh()
        {
            UpdateTileSetPairs();
            Renderer.GenerateMeshes(Slice);
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