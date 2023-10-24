using Box2D.NetStandard.Collision.Shapes;
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
using TileBash.Player.Behaviour;

namespace TileBash.Player;

public class Player2D : Sprite
{
    public Body PhysicsBody
    {
        get => box2DBodyComponent.Body;
    }

    private Box2DBodyComponent box2DBodyComponent;
    private Player2DStateController stateController;
    public Vector2 Position => PhysicsBody.Position;

    public Player2DStateIdentifier State
    {
        get => stateController.CurrentState;
    }
    public override string Name { get; set; } = "Player2D";
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
        CreateStateController();
        AttachDebugWatches();

        collidersIntervalRunner = AddEntity(new IntervalRunner(0.25f, GenerateTileColliders));
    }

    private void AttachDebugWatches()
    {
        Engine.Debugger.GeneralDebugger.AddWatch("State", this.Name, () => State);
    }

    private void CreateStateController()
    {
        stateController = AddComponent<Player2DStateController>();
        stateController.RegisterBehaviour(
            Player2DStateIdentifier.Idle,
            new Behaviour.States.PlayerIdleBehaviour(stateController)
        );
        stateController.RegisterBehaviour(
            Player2DStateIdentifier.Walking,
            new Behaviour.States.PlayerWalkingBehaviour(stateController)
        );
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
        PhysicsBody.SetTransform(
            new Vector2(
                map.Width / 2.0f * TileMapChunk.WIDTH,
                map.Height / 2.0f * TileMapChunk.HEIGHT
            ),
            0.0f
        );
    }

    private void CreateSprite()
    {
        var sheet = (
            new Spritesheet(
                Engine.Content.LoadTexture("content/spritesheet.png"),
                new Vector2(16)
            )
        );

        sheet.AddAnimationRange(new (string, Vector2, int, float, Vector2?)[] {
            ("walk_up", new Vector2(0, 0), 4, 0.1f, null),
            ("walk_up", new Vector2(0, 0), 4, 0.1f, null),
            ("walk_down", new Vector2(4, 0), 4, 0.1f, null),
            ("walk_left", new Vector2(0, 1), 4, 0.1f, null),
            ("walk_right", new Vector2(4, 1), 4, 0.1f, null),
            ("idle", new Vector2(4, 0), 1, 0.1f, null)
        });

        ConfigureSpritesheetAndDefaultAnimation(sheet, "idle");

        IsAnimated = true;
        Size = new Vector2(1.0f);
    }

    public override void Update(float dt)
    {
        UpdateTileColliders(dt);

        base.Update(dt);
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        foreach (var tile in visibleTiles)
            tile.Draw(dt, ref options);

        base.Draw(dt, ref options);
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
}
