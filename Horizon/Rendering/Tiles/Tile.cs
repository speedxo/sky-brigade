﻿using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Horizon.Primitives;

namespace Horizon.Rendering;

public partial class Tiling<TTileID, TTextureID>
    where TTileID : Enum
    where TTextureID : Enum
{
    public abstract class Tile : IUpdateable, IDrawable
    {
        public const float TILE_WIDTH = 1.0f;
        public const float TILE_HEIGHT = 1.0f;

        public Vector2 LocalPosition { get; protected set; }
        public Vector2 GlobalPosition { get; protected set; }
        public TTileID ID { get; protected set; }

        public bool ShouldUpdateMesh { get; set; }
        public bool HasCollider { get; private set; }
        public TileRenderingData RenderingData;
        public TileBox2DData Box2DData;

        public TileMapChunk Chunk { get; init; }
        public TileMap Map => Chunk.Map;

        public TileSet Set => Map.GetTileSetFromTileTextureID(RenderingData.TextureID);

        public Tile(TileMapChunk chunk, Vector2 local)
        {
            this.Chunk = chunk;
            this.LocalPosition = local;
            this.GlobalPosition = local + new Vector2(chunk.Slice * TileMapChunk.Width, 0);
            this.RenderingData = new();
            this.Box2DData = new();
        }

        public bool TryGenerateCollider()
        {
            if (Chunk.Body is null) return false;

            Box2DData = new()
            {
                Fixture = Chunk.Body.CreateFixture(new PolygonShape(
                GlobalPosition + new Vector2(TILE_WIDTH / -2.0f, TILE_HEIGHT / -2.0f),
                        GlobalPosition + new Vector2(TILE_WIDTH + TILE_WIDTH / -2.0f, TILE_HEIGHT / -2.0f),
                        GlobalPosition + new Vector2(TILE_WIDTH + TILE_WIDTH / -2.0f, TILE_HEIGHT + TILE_HEIGHT / -2.0f),
                        GlobalPosition + new Vector2(TILE_WIDTH / -2.0f, TILE_HEIGHT + TILE_HEIGHT / -2.0f)
                    )),
                Age = 0,
                Distance = 0
            };

            Box2DData.Fixture.m_friction = 1.0f;

            HasCollider = true;

            return true;
        }

        public bool TryDestroyCollider()
        {
            if (Chunk.Body is null || !HasCollider) return false;

            Chunk.Body.DestroyFixture(Box2DData.Fixture);
            HasCollider = false;

            return true;
        }

        public virtual void Draw(float dt, RenderOptions? renderOptions = null)
        {

        }
        public virtual void Update(float dt)
        {

        }

        public virtual void PostGeneration(int x, int y)
        {

        }
    }
}

