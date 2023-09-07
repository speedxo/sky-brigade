using Box2D.NetStandard.Dynamics.Bodies;
using Horizon.Primitives;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileMapChunkSlice
    {
        /// <summary>
        /// The width of the tile map chunk in tiles.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// The height of the tile map chunk in tiles.
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Gets the 2D array of tiles in the chunk.
        /// </summary>
        public Tile?[] Tiles { get; init; }

        public TileMapChunkSlice(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Tiles = new Tile[Width * Height];
        }

        public Tile? this[int x, int y]
        {
            get { return Tiles[x + y * Width]; }
            set { Tiles[x + y * Width] = value; }
        }
        public Tile? this[int i]
        {
            get
            {
                if (i < 0 || i > Tiles.Length - 1)
                    return null;

                return Tiles[i];
            }
            set
            {
                if (i < 0 || i > Tiles.Length - 1)
                    throw new IndexOutOfRangeException();

                Tiles[i] = value;
            }
        }
    }

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

        public TileMapChunkSlice[] Slices { get; init; }

        /// <summary>
        /// Gets the dictionary of tile sets paired with tile arrays.
        /// </summary>
        public Dictionary<TileSet, Tile[]> TileSetPairs { get; init; }

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
        /// Gets a value indicating whether the chunk should be updated.
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets the bounds of the chunk in world space.
        /// </summary>
        public RectangleF Bounds { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the chunk is visible by the camera.
        /// </summary>
        public bool IsVisibleByCamera { get; set; } = true;

        private float _meshUpdateCooldown = 0.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="TileMapChunk"/> class.
        /// </summary>
        /// <param name="map">The parent tile map to which this chunk belongs.</param>
        /// <param name="pos">The position of the chunk in the tile map.</param>
        public TileMapChunk(TileMap map, Vector2 pos)
        {
            Map = map;
            Position = pos;

            if (map.World is not null)
                Body = map.World.CreateBody(new BodyDef { type = BodyType.Static });

            Slices = new TileMapChunkSlice[map.Depth];
            for (int i = 0; i < Slices.Length; i++)
                Slices[i] = new(Width, Height);

            TileSetPairs = new Dictionary<TileSet, Tile[]>();

            Renderer = new TilemapRenderer(this);
            IsDirty = true;

            Bounds = new RectangleF(
                pos * new Vector2(Width - 1, Height - 1)
                    - new Vector2(Tile.TILE_WIDTH / 2.0f, Tile.TILE_HEIGHT / 2.0f),
                new(Width - 1, Height - 1)
            );
        }

        public TileMapChunkSlice CreateSlice() => new(Width, Height);

        /// <summary>
        /// Checks if a specific tile location in the chunk is empty.
        /// </summary>
        /// <param name="x">The X coordinate of the tile in the chunk.</param>
        /// <param name="y">The Y coordinate of the tile in the chunk.</param>
        /// <returns>True if the tile location is empty; otherwise, false.</returns>
        public bool IsEmpty(int x, int y, int z)
        {
            return this[x, y, z] is null;
        }

        /// <summary>
        /// Gets or sets a tile at the specified coordinates in the chunk.
        /// </summary>
        /// <param name="x">The X coordinate of the tile in the chunk.</param>
        /// <param name="y">The Y coordinate of the tile in the chunk.</param>
        /// <returns>The tile at the specified coordinates in the chunk.</returns>
        public Tile? this[int x, int y, int z]
        {
            get
            {
                if (z < 0 || z > Map.Depth - 1)
                    return null;

                return Slices[z][x, y];
            }
            set
            {
                if (z < 0 || z > Map.Depth - 1)
                    return;

                Slices[z][x, y] = value;
            }
        }

        /// <summary>
        /// Draws the chunk.
        /// </summary>
        /// <param name="dt">The time elapsed since the last frame.</param>
        /// <param name="renderOptions">Optional rendering options.</param>
        public void Draw(float dt, RenderOptions? renderOptions = null)
        {
            _meshUpdateCooldown += dt;

            if (IsDirty && _meshUpdateCooldown > 0.25f)
            {
                _meshUpdateCooldown = 0.0f;
                IsDirty = false;

                GenerateMesh();
            }

            Renderer.Draw(dt, renderOptions);
        }

        /// <summary>
        /// Flags the chunk for mesh regeneration.
        /// </summary>
        /// <returns>True if the chunk was flagged for regeneration; otherwise, false.</returns>
        public bool MarkDirty()
        {
            if (IsDirty)
                return false;

            return IsDirty = true;
        }

        /// <summary>
        /// Updates the chunk.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void Update(float dt)
        {
            if (!IsVisibleByCamera)
                return;
        }

        /// <summary>
        /// Generates the mesh for the chunk.
        /// </summary>
        public void GenerateMesh()
        {
            UpdateTileSetPairs();
            Renderer.GenerateMeshes();
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
            var tempPairs = new Dictionary<TileSet, List<Tile>>();

            for (int s = 0; s < Slices.Length; s++)
            {
                // Iterate through the tiles in the chunk.
                for (int i = 0; i < Slices[s].Tiles.Length; i++)
                {
                    var tile = Slices[s][i];
                    if (tile is null)
                        continue;

                    // Check if the tile set is already in the temporary pairs.
                    if (!tempPairs.ContainsKey(tile.Set))
                    {
                        // If not, create a new array for that tile set.
                        tempPairs[tile.Set] = new List<Tile>();
                    }

                    // Add the tile to the corresponding tile set's array.
                    tempPairs[tile.Set].Add(tile);
                }
            }

            // Copy the organized tile set pairs to the main TileSetPairs dictionary.
            foreach ((TileSet set, List<Tile> tiles) in tempPairs)
            {
                TileSetPairs[set] = tiles.ToArray();
                tiles.Clear();
            }

            // Clear the temporary storage to free up memory.
            tempPairs.Clear();
        }

        /// <summary>
        /// Populates the chunk with tiles using a custom action.
        /// </summary>
        /// <param name="action">The custom action to populate the chunk.</param>
        public void Populate(Action<TileMapChunkSlice[], TileMapChunk> action)
        {
            action(Slices, this);
        }

        /// <summary>
        /// Performs post-generation actions for the chunk.
        /// </summary>
        public void PostGenerate()
        {
            for (int s = 0; s < Slices.Length; s++)
            {
                var slice = Slices[s];
                for (int i = 0; i < slice.Tiles.Length; i++)
                {
                    if (slice.Tiles[i] is null)
                        continue;

                    slice.Tiles[i]!.PostGeneration();
                }
            }
        }
    }
}
