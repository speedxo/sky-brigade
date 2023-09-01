using Box2D.NetStandard.Dynamics.World;
using Horizon.GameEntity;
using Horizon.GameEntity.Components.Physics2D;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileMap : Entity
    {
        public const int WIDTH = 4;
        public const int HEIGHT = 4;

        public World? World { get; private set; }

        public TilemapChunkManager ChunkManager { get; private set; }

        public Dictionary<string, TileSet> TileSets { get; private set; }

        public TileMap()
        {
            Name = "Tilemap";
        }

        public override void Initialize()
        {
            if (Parent!.HasComponent<Box2DWorldComponent>())
                World = Parent!.GetComponent<Box2DWorldComponent>();

            TileSets = new();
            ChunkManager = AddComponent<TilemapChunkManager>();
        }

        public bool IsEmpty(int x, int y)
        {
            return this[x, y] == null;
        }

        public Tile? this[int x, int y]
        {
            get
            {
                int chunkIndexX = x / TileMapChunk.Width;
                int chunkIndexY = y / TileMapChunk.Height;

                if (chunkIndexX >= WIDTH || chunkIndexX >= HEIGHT || x < 0 || y < 0 || y >= TileMapChunk.Height)
                    return null;

                int tileIndexX = x % TileMapChunk.Width;
                int tileIndexY = y % TileMapChunk.Height;

                return ChunkManager.Chunks[chunkIndexX, chunkIndexY].Tiles[tileIndexX, tileIndexY];
            }
        }

        public TileSet AddTileSet(string name, TileSet set)
        {
            TileSets.Add(name, set);
            return set;
        }

        public void GenerateMeshes()
        {
            ChunkManager.GenerateMeshes();
        }

        public void GenerateTiles(Func<TileMapChunk, Vector2, Tile?> generateTileFunc)
        {
            ChunkManager.GenerateTiles(generateTileFunc);
        }

        public void PopulateTiles(Action<Tile?[,], TileMapChunk> action)
        {
            ChunkManager.PopulateTiles(action);
        }

        public TileSet GetTileSetFromTileTextureID(TTextureID textureID)
        {
            foreach (TileSet set in TileSets.Values)
            {
                if (set.ContainsTextureID(textureID))
                {
                    return set;
                }
            }

            GameManager.Instance.Logger.Log(Logging.LogLevel.Fatal, $"[TileMap] No TileSet is bound to the texture ID '{textureID}'!");
            return null; // Returning null because LogLevel.Fatal throws an exception.
        }
    }
}