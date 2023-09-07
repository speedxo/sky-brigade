﻿using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using Horizon;
using Horizon.Extentions;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using System.Collections;
using System.Numerics;

namespace Game2D;

public class Player2D : Sprite
{
    public Body PhysicsBody
    {
        get => box2DBodyComponent.Body;
    }

    private const float speed = 50.0f;
    private Box2DBodyComponent box2DBodyComponent;

    public Vector2 Position => PhysicsBody.Position;

    private readonly World world;
    private readonly TileMap map;
    private readonly IntervalRunner collidersIntervalRunner;
    private readonly List<Tile> colliableTiles = new();
    private Tile[] visibleTiles = Array.Empty<Tile>();

    public Player2D(World world, TileMap map)
    {
        this.world = world;
        this.map = map;

        CreateSprite();
        CreatePhysics();

        collidersIntervalRunner = AddEntity(new IntervalRunner(0.25f, GenerateTileColliders));
    }

    private void CreatePhysics()
    {
        box2DBodyComponent = AddComponent(
            new Box2DBodyComponent(
                world.CreateBody(
                    new BodyDef { type = BodyType.Dynamic, position = new Vector2(-5, 16) }
                )
            )
        );

        CircleShape shape = new() { Radius = 0.6f };

        PhysicsBody.CreateFixture(shape, 1.0f);
        PhysicsBody.SetFixedRotation(true);
        PhysicsBody.SetLinearDampling(7.5f);
    }

    private void CreateSprite()
    {
        var sprSheet1 = (
            new Spritesheet(
                GameManager.Instance.ContentManager.LoadTexture("content/spritesheet.png"),
                new Vector2(16)
            )
        );
        sprSheet1.AddAnimation("walk_up", new Vector2(0, 0), 4);
        sprSheet1.AddAnimation("walk_down", new Vector2(4, 0), 4);

        sprSheet1.AddAnimation("walk_left", new Vector2(0, 1), 4);
        sprSheet1.AddAnimation("walk_right", new Vector2(4, 1), 4);
        sprSheet1.AddAnimation("idle", new Vector2(4, 0), 1);

        Setup(sprSheet1, "idle");

        IsAnimated = true;
        // nice
        Size = new Vector2(1.0f);
    }

    public override void Update(float dt)
    {
        UpdateTileColliders(dt);
        UpdatePosition(dt);

        base.Update(dt);
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        foreach (var tile in visibleTiles)
            tile.Draw(dt, renderOptions);

        base.Draw(dt, renderOptions);
    }

    private void GenerateTileColliders()
    {
        // Enumerate the enumerable so its only itterated once.
        visibleTiles = map.FindVisibleTiles(Position - Size / 2.0f, 8.0f)
            .Where(e => e.PhysicsData.IsCollidable)
            .ToArray();

        foreach (var tile in visibleTiles)
        {
            tile.PhysicsData.Age = 0;

            if (!colliableTiles.Contains(tile))
                if (tile.TryGenerateCollider())
                    colliableTiles.Add(tile);
        }
    }

    private void UpdateTileColliders(float dt)
    {
        for (int i = 0; i < colliableTiles.Count; i++)
        {
            var tile = colliableTiles[i];
            tile.Update(dt);

            tile.PhysicsData.Distance = Vector2.DistanceSquared(tile.GlobalPosition, Position);
            if (tile.PhysicsData.Distance > 25.0f)
            {
                tile.PhysicsData.Age += dt;

                if (tile.PhysicsData.Age > 5.0f)
                {
                    if (tile.TryDestroyCollider())
                    {
                        tile.PhysicsData.Age = 0.0f;
                        colliableTiles.Remove(tile);
                    }
                }
            }
        }
    }

    private void UpdatePosition(float dt)
    {
        // Move player with controller
        var movementDir = GameManager.Instance.InputManager.GetVirtualController().MovementAxis;

        PhysicsBody.ApplyForce(movementDir * speed, Transform.Position);
        PhysicsBody.SetLinearVelocity(
            Vector2.Clamp(PhysicsBody.GetLinearVelocity(), Vector2.One * -5, Vector2.One * 5)
        );

        FrameName = movementDir switch
        {
            var v when v.X < 0 => "walk_left",
            var v when v.X > 0 => "walk_right",
            var v when v.Y > 0 => "walk_up",
            var v when v.Y < 0 => "walk_down",
            _ => "idle"
        };
    }
}
