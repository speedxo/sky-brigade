using Box2D.NetStandard.Dynamics.Bodies;
using Horizon.Primitives;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    /// <summary>
    /// Represents a chunk of tiles in a tile map.
    /// </summary>
    public class TileMapChunk : IDrawable, IUpdateable
    {
        /// <summary>
        /// Gets or sets the physics body associated with the chunk.
        /// </summary>
        public Body? Body { get; set; }

        /// <summary>
        /// The width of the tile map chunk in tiles.
        /// </summary>
        public const int Width = 32;

        /// <summary>
        /// The height of the tile map chunk in tiles.
        /// </summary>
        public const int Height = 32;

        /// <summary>
        /// Gets the 2D array of tiles in the chunk.
        /// </summary>
        public Tile?[,] Tiles { get; init; }

        /// <summary>
        /// Gets the dictionary of tile sets paired with tile arrays.
        /// </summary>
        public Dictionary<TileSet, Tile[,]> TileSetPairs { get; init; }

        /// <summary>
        /// Gets the position of the chunk in the tile map.
        /// </summary>
        public Vector2 Position { get; init; }

        /// <summary>
        /// Gets the parent tile map to which this chunk belongs.
        /// </summary>
        public TileMap Map { get; init; }

        /// <summary>
        /// Gets the renderer responsible for rendering the chunk.
        /// </summary>
        public TilemapRenderer Renderer { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the chunk should be updated.
        /// </summary>
        public bool ShouldUpdate { get; set; }

        /// <summary>
        /// Gets the bounds of the chunk in world space.
        /// </summary>
        public RectangleF Bounds { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the chunk is visible by the camera.
        /// </summary>
        public bool IsVisibleByCamera { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileMapChunk"/> class.
        /// </summary>
        /// <param name="map">The parent tile map to which this chunk belongs.</param>
        /// <param name="pos">The position of the chunk in the tile map.</param>
        public TileMapChunk(TileMap map, Vector2 pos)
        {
            Map = map;
            Position = pos;

            if (map.World is not null) Body = map.World.CreateBody(new BodyDef
            {
                type = BodyType.Static
            });

            Tiles = new Tile?[Width, Height];
            TileSetPairs = new Dictionary<TileSet, Tile[,]>();
            Renderer = new TilemapRenderer(this);
            ShouldUpdate = true;

            Bounds = new RectangleF(pos * new Vector2(Width - 1, Height - 1) - new Vector2(Tile.TILE_WIDTH / 2.0f, Tile.TILE_HEIGHT / 2.0f), new(Width - 1, Height - 1));
        }

        /// <summary>
        /// Checks if a specific tile location in the chunk is empty.
        /// </summary>
        /// <param name="x">The X coordinate of the tile in the chunk.</param>
        /// <param name="y">The Y coordinate of the tile in the chunk.</param>
        /// <returns>True if the tile location is empty; otherwise, false.</returns>
        public bool IsEmpty(int x, int y)
        {
            return this[x, y] is null;
        }

        /// <summary>
        /// Gets or sets a tile at the specified coordinates in the chunk.
        /// </summary>
        /// <param name="x">The X coordinate of the tile in the chunk.</param>
        /// <param name="y">The Y coordinate of the tile in the chunk.</param>
        /// <returns>The tile at the specified coordinates in the chunk.</returns>
        public Tile? this[int x, int y]
        {
            get
            {
                if (x / Width >= TileMap.WIDTH || x < 0 || y < 0 || y / Height >= TileMap.HEIGHT)
                    return null;

                return Tiles[x, y];
            }
        }

        /// <summary>
        /// Draws the chunk.
        /// </summary>
        /// <param name="dt">The time elapsed since the last frame.</param>
        /// <param name="renderOptions">Optional rendering options.</param>
        public void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (ShouldUpdate)
                GenerateMesh();

            Renderer.Draw(dt, renderOptions);
        }

        /// <summary>
        /// Flags the chunk for mesh regeneration.
        /// </summary>
        /// <returns>True if the chunk was flagged for regeneration; otherwise, false.</returns>
        public bool FlagForMeshRegeneration()
        {
            if (ShouldUpdate)
                return false;

            ShouldUpdate = true;
            return true;
        }

        /// <summary>
        /// Updates the chunk.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void Update(float dt)
        {
            if (!IsVisibleByCamera) return;

            // TODO: Implement update logic for the chunk.
        }

        /// <summary>
        /// Generates the mesh for the chunk.
        /// </summary>
        public void GenerateMesh()
        {
            UpdateTileSetPairs();
            Renderer.GenerateMeshes();
            ShouldUpdate = false;
        }

        /// <summary>
        /// Updates the association between tile sets and their corresponding tiles in the chunk.
        /// </summary>
        /// <remarks>
        /// This method iterates through the tiles in the chunk and organizes them into pairs, where each tile set
        /// is associated with a 2D array of tiles that belong to that set. This allows for efficient rendering of tiles
        /// using tile sets and minimizes the number of draw calls required.
        /// </remarks>
        private void UpdateTileSetPairs()
        {
            // Clear the existing tile set pairs to rebuild them.
            TileSetPairs.Clear();

            // Temporary storage for organizing tiles by tile set.
            var tempPairs = new Dictionary<TileSet, Tile[,]>();

            // Iterate through the tiles in the chunk.
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var tile = Tiles[x, y];
                    if (tile == null) continue;

                    // Check if the tile set is already in the temporary pairs.
                    if (!tempPairs.ContainsKey(tile.Set))
                    {
                        // If not, create a new 2D array for that tile set.
                        tempPairs[tile.Set] = new Tile[Width, Height];
                    }

                    // Add the tile to the corresponding tile set's array.
                    tempPairs[tile.Set][x, y] = tile;
                }
            }

            // Copy the organized tile set pairs to the main TileSetPairs dictionary.
            foreach ((TileSet set, Tile[,] tiles) in tempPairs)
            {
                TileSetPairs[set] = tiles;
            }

            // Clear the temporary storage to free up memory.
            tempPairs.Clear();
        }


        /// <summary>
        /// Generates tiles for the chunk using a custom generation function.
        /// </summary>
        /// <param name="generateTileFunc">The custom generation function.</param>
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
        /// Populates the chunk with tiles using a custom action.
        /// </summary>
        /// <param name="action">The custom action to populate the chunk.</param>
        public void Populate(Action<Tile?[,], TileMapChunk> action)
        {
            action(Tiles, this);
        }

        /// <summary>
        /// Performs post-generation actions for the chunk.
        /// </summary>
        public void PostGenerate()
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    if (Tiles[x, y] is null) continue;

                    Tiles[x, y]!.PostGeneration();
                }
            }
        }
    }
}