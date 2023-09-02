using Box2D.NetStandard.Collision.Shapes;
using Horizon.Primitives;
using System.Numerics;

namespace Horizon.Rendering;

/// <summary>
/// Represents a tile in a tilemap.
/// </summary>
/// <typeparam name="TTileID">The type of tile ID.</typeparam>
/// <typeparam name="TTextureID">The type of texture ID.</typeparam>
public partial class Tiling<TTileID, TTextureID>
    where TTileID : Enum
    where TTextureID : Enum
{
    public abstract class Tile : IUpdateable, IDrawable
    {
        /// <summary>
        /// The width of the tile.
        /// </summary>
        public const float TILE_WIDTH = 1.0f;

        /// <summary>
        /// The height of the tile.
        /// </summary>
        public const float TILE_HEIGHT = 1.0f;

        /// <summary>
        /// Gets the local position of the tile within its chunk.
        /// </summary>
        public Vector2 LocalPosition { get; protected set; }

        /// <summary>
        /// Gets the global position of the tile within the tilemap.
        /// </summary>
        public Vector2 GlobalPosition { get; protected set; }

        /// <summary>
        /// Gets the ID of the tile.
        /// </summary>
        public TTileID ID { get; protected set; }

        /// <summary>
        /// Gets the rendering data for this tile.
        /// </summary>
        public TileRenderingData RenderingData = default;

        /// <summary>
        /// Gets the Box2D data for this tile.
        /// </summary>
        public TilePhysicsData PhysicsData = default;

        /// <summary>
        /// Gets the chunk to which this tile belongs.
        /// </summary>
        public TileMapChunk Chunk { get; init; }

        /// <summary>
        /// Gets the tilemap to which this tile belongs.
        /// </summary>
        public TileMap Map => Chunk.Map;

        /// <summary>
        /// Gets the tile set to which this tile belongs.
        /// </summary>
        public TileSet Set => Map.GetTileSetFromTileTextureID(RenderingData.TextureID);

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile{TTileID, TTextureID}"/> class.
        /// </summary>
        /// <param name="chunk">The chunk to which the tile belongs.</param>
        /// <param name="local">The local position of the tile within the chunk.</param>
        public Tile(TileMapChunk chunk, Vector2 local)
        {
            Chunk = chunk;
            LocalPosition = local;
            GlobalPosition = local + chunk.Position * new Vector2(TileMapChunk.Width - 1, TileMapChunk.Height - 1);
            RenderingData = new TileRenderingData();
            PhysicsData = new TilePhysicsData();
        }

        /// <summary>
        /// Tries to generate a collider for this tile.
        /// </summary>
        /// <returns>True if a collider was successfully generated; otherwise, false.</returns>
        public virtual bool TryGenerateCollider()
        {
            if (Chunk.Body is null || !PhysicsData.IsCollidable)
                return false;

            PhysicsData = PhysicsData with
            {
                Fixture = Chunk.Body.CreateFixture(GenerateCollider()),
                Age = 0,
                Distance = 0
            };

            PhysicsData.Fixture.m_friction = 0.6f;
            PhysicsData.HasCollider = true;

            return true;
        }

        /// <summary>
        /// Generates the collider shape for this tile.
        /// </summary>
        /// <returns>The collider shape.</returns>
        protected virtual PolygonShape GenerateCollider()
        {
            return new PolygonShape(
                GlobalPosition + new Vector2(TILE_WIDTH / -2.0f, TILE_HEIGHT / -2.0f),
                GlobalPosition + new Vector2(TILE_WIDTH + TILE_WIDTH / -2.0f, TILE_HEIGHT / -2.0f),
                GlobalPosition + new Vector2(TILE_WIDTH + TILE_WIDTH / -2.0f, TILE_HEIGHT + TILE_HEIGHT / -2.0f),
                GlobalPosition + new Vector2(TILE_WIDTH / -2.0f, TILE_HEIGHT + TILE_HEIGHT / -2.0f)
            );
        }

        /// <summary>
        /// Tries to destroy the collider of this tile.
        /// </summary>
        /// <returns>True if the collider was successfully destroyed; otherwise, false.</returns>
        public virtual bool TryDestroyCollider()
        {
            if (Chunk.Body is null || !PhysicsData.HasCollider)
                return false;

            Chunk.Body.DestroyFixture(PhysicsData.Fixture);
            PhysicsData.HasCollider = false;

            return true;
        }

        /// <summary>
        /// Draws the tile.
        /// </summary>
        /// <param name="dt">The time elapsed since the last frame.</param>
        /// <param name="renderOptions">Optional rendering options.</param>
        public virtual void Draw(float dt, RenderOptions? renderOptions = null)
        {
            // Implement drawing logic here.
        }

        /// <summary>
        /// Updates the tile.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public virtual void Update(float dt)
        {
            // Implement update logic here.
        }

        /// <summary>
        /// Performs post-generation actions for the tile.
        /// </summary>
        public virtual void PostGeneration()
        {
            // Implement post-generation logic here.
        }
    }
}