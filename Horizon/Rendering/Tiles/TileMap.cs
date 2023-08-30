using System.Numerics;
using Box2D.NetStandard.Dynamics.World;
using Horizon.GameEntity;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileMap : Entity
    {
        public const int WIDTH = 4;

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
                if (x / TileMapChunk.Width >= ChunkManager.Chunks.Length|| x < 0 || y < 0 || y >= TileMapChunk.Height)
                    return null;

                int chunkIndex = x / TileMapChunk.Width;
                int tileIndex = x % TileMapChunk.Width;

                return ChunkManager.Chunks[chunkIndex].Tiles[tileIndex, y];
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