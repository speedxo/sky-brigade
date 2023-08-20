using System.Numerics;
using Horizon.GameEntity;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileMap : Entity
    {
        public const int WIDTH = 4;

        public TilemapChunkManager ChunkManager { get; init; }

        public Dictionary<string, TileSet> Tilesets { get; init; }

        public TileMap()
        {
            Name = "Tilemap";

            Tilesets = new();
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
                    if (x / WIDTH > WIDTH - 1) return null;
                    if (x < 0 || y < 0 || x > WIDTH * TileMapChunk.WIDTH - 1 || y > TileMapChunk.HEIGHT - 1) return null;

                    return ChunkManager.Chunks[x / WIDTH].Tiles[x % TileMapChunk.WIDTH, y];
                }
            }

        public TileSet AddTileSet(string name, TileSet set)
        {
            Tilesets.Add(name, set);
            return set;
        }

        public void GenerateMeshes()
        {
            ChunkManager.GenerateMeshes();
        }
        /// <summary>
        /// This method accepts a generator function that is called for each position in the tilemap.
        /// </summary>
        /// <param name="generateTileFunc">The generator function</param>
        public void GenerateTiles(Func<TileMapChunk, Vector2, Tile?> generateTileFunc)
        {
            ChunkManager.GenerateTiles(generateTileFunc);
        }

        /// <summary>
        /// This method accepts a populator action that is expected to fill the tile[].
        /// </summary>
        /// <param name="action">The populator action</param>
        public void PopulateTiles(Action<Tile?[,], TileMapChunk> action)
        {
            ChunkManager.PopulateTiles(action);
        }

        public TileSet GetTileSetFromTileTextureID(TTextureID textureID)
        {
            foreach (TileSet set in Tilesets.Values)
            {
                if (set.ContainsTextureID(textureID))
                {
                    return set;
                }
            }

            GameManager.Instance.Logger.Log(Logging.LogLevel.Fatal, $"[TileMap] No TileSet is bound to the texture ID '{textureID}'"!);
            return null; // Returning null because LogLevel.Fatal throws an exception.
        }
    }

}