using Box2D.NetStandard.Dynamics.World;
using Horizon.GameEntity;
using Horizon.GameEntity.Components.Physics2D;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    /// <summary>
    /// Represents a tile map in the game world.
    /// </summary>
    public class TileMap : Entity
    {
        /// <summary>
        /// The width of the tile map in chunks.
        /// </summary>
        public const int WIDTH = 4;

        /// <summary>
        /// The height of the tile map in chunks.
        /// </summary>
        public const int HEIGHT = 4;

        /// <summary>
        /// Gets or sets the physics world associated with the tile map.
        /// </summary>
        public World? World { get; private set; }

        /// <summary>
        /// Gets the chunk manager responsible for managing tilemap chunks.
        /// </summary>
        public TilemapChunkManager ChunkManager { get; private set; }

        /// <summary>
        /// Gets the dictionary of tile sets used in the tile map.
        /// </summary>
        public Dictionary<string, TileSet> TileSets { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileMap"/> class.
        /// </summary>
        public TileMap()
        {
            Name = "Tilemap";
        }

        /// <summary>
        /// Initializes the tile map.
        /// </summary>
        public override void Initialize()
        {
            if (Parent!.HasComponent<Box2DWorldComponent>())
                World = Parent!.GetComponent<Box2DWorldComponent>();

            TileSets = new Dictionary<string, TileSet>();
            ChunkManager = AddComponent<TilemapChunkManager>();
        }

        /// <summary>
        /// Checks if a specific tile location is empty.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <returns>True if the tile location is empty; otherwise, false.</returns>
        public bool IsEmpty(int x, int y)
        {
            return this[x, y] == null;
        }

        /// <summary>
        /// Returns all the tiles within a half area by half area region around a specified point, within O(area) time complexity.
        /// </summary>
        public IEnumerable<Tile> FindVisibleTiles(Vector2 position, float area = 10.0f)
        {
            var areaSize = new Vector2(area / 2.0f);
            var playerPos = position + areaSize / 2.0f;

            int startingX = (int)(playerPos.X - areaSize.X);
            int endingX = (int)(playerPos.X + areaSize.X);

            int startingY = (int)(playerPos.Y - areaSize.Y);
            int endingY = (int)(playerPos.Y + areaSize.Y);

            for (int x = startingX; x < endingX; x++)
            {
                for (int y = endingY; y > startingY; y--)
                {
                    var tile = this[x, y];
                    if (tile != null)
                    {
                        yield return tile;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a tile at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <returns>The tile at the specified coordinates.</returns>
        public Tile? this[int x, int y]
        {
            get
            {
                int chunkIndexX = x / (TileMapChunk.Width - 1);
                int chunkIndexY = y / (TileMapChunk.Height - 1);

                if (chunkIndexX >= WIDTH || chunkIndexY >= HEIGHT || x < 0 || y < 0)
                    return null;

                int tileIndexX = x % (TileMapChunk.Width - 1);
                int tileIndexY = y % (TileMapChunk.Height - 1);

                return ChunkManager.Chunks[chunkIndexX + chunkIndexY * WIDTH].Tiles[tileIndexX + tileIndexY * TileMapChunk.Width];
            }
        }

        /// <summary>
        /// Adds a tile set to the tile map.
        /// </summary>
        /// <param name="name">The name of the tile set.</param>
        /// <param name="set">The tile set to add.</param>
        /// <returns>The added tile set.</returns>
        public TileSet AddTileSet(string name, TileSet set)
        {
            TileSets.Add(name, set);
            return set;
        }

        /// <summary>
        /// Generates meshes for the tile map.
        /// </summary>
        public void GenerateMeshes()
        {
            ChunkManager.GenerateMeshes();
        }

        /// <summary>
        /// Generates tiles for the tile map using a custom generation function.
        /// </summary>
        /// <param name="generateTileFunc">The custom generation function.</param>
        public void GenerateTiles(Func<TileMapChunk, Vector2, Tile?> generateTileFunc)
        {
            ChunkManager.GenerateTiles(generateTileFunc);
        }

        /// <summary>
        /// Populates tiles in the tile map using a custom action.
        /// </summary>
        /// <param name="action">The custom action to populate tiles.</param>
        public void PopulateTiles(Action<Tile?[,], TileMapChunk> action)
        {
            ChunkManager.PopulateTiles(action);
        }
        /// <summary>
        /// Populates tiles in the tile map using a custom action.
        /// </summary>
        /// <param name="action">The custom action to populate tiles.</param>
        public void PopulateTiles(Action<Tile?[], TileMapChunk> action)
        {
            ChunkManager.PopulateTiles(action);
        }

        /// <summary>
        /// Gets the tile set associated with a given texture ID.
        /// </summary>
        /// <param name="textureID">The texture ID to search for.</param>
        /// <returns>The tile set associated with the texture ID, or null if not found.</returns>
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
            return null!; // Returning null because LogLevel.Fatal throws an exception.
        }
    }
}