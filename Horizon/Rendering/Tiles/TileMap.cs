using Box2D.NetStandard.Dynamics.World;
using Horizon.GameEntity;
using Horizon.GameEntity.Components.Physics2D;
using System.Numerics;
using TiledSharp;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    /// <summary>
    /// Represents a tile map in the game world.
    /// </summary>
    public class TileMap : Entity
    {
        /// <summary>
        /// The depth of the tilemap in slices.
        /// </summary>
        public int Depth { get; init; }

        /// <summary>
        /// The width of the tile map in chunks.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// The height of the tile map in chunks.
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Gets or sets the physics world associated with the tile map.
        /// </summary>
        public World? World { get; private set; }

        /// <summary>
        /// Gets the chunk manager responsible for managing tilemap chunks.
        /// </summary>
        public TileMapChunkManager ChunkManager { get; private set; }

        /// <summary>
        /// Gets the dictionary of tile sets used in the tile map.
        /// </summary>
        public Dictionary<string, TileSet> TileSets { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileMap"/> class.
        /// </summary>
        /// <param name="width">The width of the tilemap in chunks (32)</param>
        /// <param name="height"></param>
        public TileMap(int width, int height, int depth)
        {
            Name = "Tilemap";

            this.Depth = depth;
            this.Width = width;
            this.Height = height;

            TileSets = new Dictionary<string, TileSet>();
        }

        /// <summary>
        /// Creates an instance of <see cref="TileMap"/> and poplates its layers, chunks and tilesets from the specified tilemap verbatim.
        /// </summary>
        /// <param name="parent">The gamescreen (necessary if you plan to use Box2D integration).</param>
        /// <param name="tiledMapPath">The path of the tiled map.</param>
        /// <returns></returns>
        public static TileMap? FromTiledMap(Entity parent, string tiledMapPath)
        {
            try
            {
                var tiledMap = new TmxMap(tiledMapPath);

                if (tiledMap.Width <= 0 || tiledMap.Height <= 0)
                {
                    throw new ArgumentException("Invalid Tiled map dimensions.");
                }

                int widthInChunks = tiledMap.Width / TileMapChunk.Width;
                int heightInChunks = tiledMap.Height / TileMapChunk.Height;
                int depthInLayers = tiledMap.Layers.Count;

                var map = new TileMap(widthInChunks, heightInChunks, depthInLayers)
                {
                    Parent = parent
                };
                map.Initialize();

                foreach (var tileset in tiledMap.Tilesets)
                {
                    if (!string.IsNullOrEmpty(tileset.Image?.Source))
                    {
                        var set = new TileSet(GameManager.Instance.ContentManager.LoadTexture(tileset.Image.Source), new Vector2(tileset.TileWidth, tileset.TileHeight))
                        {
                            ID = tileset.FirstGid,
                            TileCount = tileset.TileCount
                        };
                        map.AddTileSet(tileset.Name, set);
                    }
                }

                int layerIndex = 0;

                int chunkWidth = TileMapChunk.Width;
                int chunkHeight = TileMapChunk.Height;

                foreach (var layer in tiledMap.Layers)
                {
                    layer.Properties.TryGetValue("isCollidable", out var _stringIsCollidable);

                    bool isCollidable = bool.TryParse(_stringIsCollidable, out isCollidable) && isCollidable;

                    foreach (var tile in layer.Tiles)
                    {
                        if (tile.Gid == 0)
                            continue;

                        // invert the tile Y coordinates because one again openGL is weird (read about coordinate system orientations)
                        int tileY = tiledMap.Height - tile.Y - 1;

                        int localTileX = (tile.X % chunkWidth);
                        int localTileY = (tileY % chunkHeight);

                        int chunkX = tile.X / chunkWidth;
                        int chunkY = tileY / chunkHeight;

                        var (set, id) = map.FindTilesetFromGUID(tile.Gid);
                        var chunk = map.ChunkManager[chunkX, chunkY];

                        var config = new StaticTile.TiledTileConfig
                        {
                            ID = id,
                            Set = set,
                            IsCollidable = isCollidable,
                            IsVisible = layer.Visible
                        };

                        map[tile.X, tileY, layerIndex] = new StaticTile(config, chunk, new Vector2(localTileX, localTileY));
                    }
                    layerIndex++;
                }

                map.ChunkManager.PostGenerateTiles();

                return map;
            }
            catch (Exception ex)
            {
                GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Error loading Tiled map: + {ex.Message}");
                return null;
            }
        }
            

        private (TileSet? set, int localTileID) FindTilesetFromGUID(int guid)
        {
            TileSet? set = null;
            int localTileId = -1;

            foreach (var tileset in TileSets.Values)
            {
                if (guid >= tileset.ID && guid < tileset.ID + tileset.TileCount)
                {
                    localTileId = guid - tileset.ID;
                    set = tileset;
                    break; // Exit the loop once a matching tileset is found
                }
            }
            return (set, localTileId);
        }


        /// <summary>
        /// Initializes the tile map.
        /// </summary>
        public override void Initialize()
        {
            if (Parent!.HasComponent<Box2DWorldComponent>())
                World = Parent!.GetComponent<Box2DWorldComponent>();
            ChunkManager = AddComponent<TileMapChunkManager>();
        }

        /// <summary>
        /// Checks if a specific tile location is empty.
        /// </summary>
        /// <param name="x">The X coordinate of the tile.</param>
        /// <param name="y">The Y coordinate of the tile.</param>
        /// <returns>True if the tile location is empty; otherwise, false.</returns>
        public bool IsEmpty(int x, int y, int z = 0)
        {
            return this[x, y, z] is null;
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
                for (int y = startingY; y < endingY; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        if (!IsEmpty(x, y, z))
                            yield return this[x, y, z]!;
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
        public Tile? this[int x, int y, int z]
        {
            get
            {
                int chunkIndexX = x / (TileMapChunk.Width);
                int chunkIndexY = y / (TileMapChunk.Height);

                if (chunkIndexX >= Width || chunkIndexY >= Height || x < 0 || y < 0)
                    return null;

                int tileIndexX = x % (TileMapChunk.Width);
                int tileIndexY = y % (TileMapChunk.Height);

                return ChunkManager.Chunks[chunkIndexX + chunkIndexY * Width][tileIndexX, tileIndexY, z];
            }
            set
            {
                int chunkIndexX = x / (TileMapChunk.Width);
                int chunkIndexY = y / (TileMapChunk.Height);

                if (chunkIndexX >= Width || chunkIndexY >= Height || x < 0 || y < 0)
                    return;

                int tileIndexX = x % (TileMapChunk.Width);
                int tileIndexY = y % (TileMapChunk.Height);

                ChunkManager.Chunks[chunkIndexX + chunkIndexY * Width][tileIndexX, tileIndexY, z] = value;
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
        /// Populates tiles in the tile map using a custom action.
        /// </summary>
        /// <param name="action">The custom action to populate tiles.</param>
        public void PopulateTiles(Action<TileMapChunkSlice[], TileMapChunk> action)
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