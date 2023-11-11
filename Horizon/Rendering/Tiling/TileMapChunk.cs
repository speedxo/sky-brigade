using Box2D.NetStandard.Dynamics.Bodies;
using Horizon.Primitives;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
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
        public const int WIDTH = 32;

        /// <summary>
        /// The height of the tile map chunk in tiles.
        /// </summary>
        public const int HEIGHT = 32;

        public TileMapChunkSlice[] Slices { get; init; }

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
        /// Gets the bounds of the chunk in world space.
        /// </summary>
        public RectangleF Bounds { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the chunk is visible by the camera.
        /// </summary>
        public bool IsVisibleByCamera { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether the chunk should be updated.
        /// </summary>
        public bool IsDirty { get; internal set; }

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
                Slices[i] = new(WIDTH, HEIGHT);

            Renderer = new TilemapRenderer(this);

            Bounds = new RectangleF(
                pos * new Vector2(WIDTH, HEIGHT)
                    - new Vector2(Tile.TILE_WIDTH / 2.0f, Tile.TILE_HEIGHT / 2.0f),
                new(WIDTH, HEIGHT)
            );

            IsDirty = true;
        }

        public TileMapChunkSlice CreateSlice() => new(WIDTH, HEIGHT);

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
        /// <param name="options">Optional rendering options.</param>
        public void Render(float dt, ref RenderOptions options)
        {
            Renderer.Draw(dt, ref options);
        }

        /// <summary>
        /// Updates the chunk.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void UpdateState(float dt)
        {
            if (!IsVisibleByCamera)
                return;
        }

        public void UpdatePhysics(float dt) { }

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
