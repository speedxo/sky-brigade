using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using Horizon.Core;
using Horizon.Core.Components.Physics2D;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using TileBash.Player.Behaviour;

namespace TileBash.Player;

public class Player2D : Sprite
{
    public static Player2D Current { get; private set; }

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

    private readonly World world;
    private readonly TileMap map;
    private readonly IntervalRunner collidersIntervalRunner;
    private readonly List<Tile> colliableTiles = new();
    private Tile[] visibleTiles = Array.Empty<Tile>();

    public Player2D(World world, TileMap map)
        : base(new Vector2(16))
    {
        Current = this;
        this.world = world;
        this.map = map;

        collidersIntervalRunner = AddEntity(new IntervalRunner(1.0f / 4.0f, GenerateTileColliders));
    }

    public override void Initialize()
    {
        CreateSprite();
        CreatePhysics();
        CreateStateController();
        AttachDebugWatches();

        base.Initialize();
    }

    private void AttachDebugWatches()
    {
        //Engine.Debugger.GeneralDebugger.AddWatch("State", this.Name, () => State);
    }

    private void CreateStateController()
    {
        stateController = AddComponent<Player2DStateController>(new(this));

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
                world.CreateBody(new BodyDef { type = BodyType.Dynamic, position = new Vector2() })
            )
        );

        CircleShape shape = new() { Radius = 0.6f };

        PhysicsBody.CreateFixture(shape, 1.0f);
        PhysicsBody.SetFixedRotation(true);
        PhysicsBody.SetLinearDampling(7.5f);
        PhysicsBody.SetTransform(
            new Vector2(
                map.Width / 2.0f * TileMapChunk.WIDTH * map.TileSize.X,
                map.Height / 2.0f * TileMapChunk.HEIGHT * map.TileSize.Y
            ),
            0.0f
        );
    }

    private void CreateSprite()
    {
        ConfigureSpriteSheet(
            SpriteSheet.FromTexture(
                Engine
                    .ContentManager
                    .Textures
                    .Create(new TextureDescription { Path = "content/spritesheet.png" })
                    .Asset,
                new Vector2(16, 16)
            ),
            "idle"
        );

        AddAnimationRange(
            new (string, Vector2, uint, float, Vector2?)[]
            {
                ("walk_up", new Vector2(0, 0), 4, 0.1f, null),
                ("walk_down", new Vector2(0, 3), 4, 0.1f, null),
                ("walk_side", new Vector2(0, 1), 4, 0.1f, null),
                ("idle", new Vector2(0, 3), 0, 0.1f, null)
            }
        );

        IsAnimated = true;
    }

    public override void UpdateState(float dt)
    {
        UpdateTileColliders(dt);

        base.UpdateState(dt);
    }

    public override void Render(float dt, object? obj = null)
    {
        //foreach (var tile in visibleTiles)
        //    tile.Render(dt, ref options);

        base.Render(dt);
    }

    private void GenerateTileColliders()
    {
        //// Enumerate the enumerable so its only iterated _once_.
        //visibleTiles = map.FindVisibleTiles(Position - Transform.Size / 2.0f, 8.0f)
        //    .Where(e => e.PhysicsData.IsCollidable)
        //    .ToArray();

        //foreach (var tile in visibleTiles)
        //{
        //    tile.PhysicsData.Age = 0;

        //    if (!colliableTiles.Contains(tile))
        //        if (tile.TryGenerateCollider())
        //            colliableTiles.Add(tile);
        //}
    }

    private void UpdateTileColliders(float dt)
    {
        //GenerateTileColliders();

        //for (int i = 0; i < colliableTiles.Count; i++)
        //{
        //    var tile = colliableTiles[i];
        //    tile.UpdateState(dt);

        //    tile.PhysicsData.Distance = Vector2.DistanceSquared(tile.GlobalPosition, Position);
        //    if (tile.PhysicsData.Distance > 25.0f)
        //    {
        //        tile.PhysicsData.Age += dt;

        //        if (tile.PhysicsData.Age > 5.0f)
        //        {
        //            if (tile.TryDestroyCollider())
        //            {
        //                tile.PhysicsData.Age = 0.0f;
        //                colliableTiles.Remove(tile);
        //            }
        //        }
        //    }
        //}
    }
}
