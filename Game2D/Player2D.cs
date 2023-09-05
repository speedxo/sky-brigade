using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using Horizon;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using System.Numerics;

namespace Game2D;

public class Player2D : Sprite
{
    public Body PhysicsBody { get => box2DBodyComponent.Body; }
    private Box2DBodyComponent box2DBodyComponent;

    public Vector2 Position => PhysicsBody.Position;

    private readonly World world;
    private readonly TileMap map;
    private readonly List<Tile> colliableTiles = new();
    private Tile[] visibleTiles = Array.Empty<Tile>();

    public Player2D(World world, TileMap map)
    {
        this.world = world;
        this.map = map;

        CreateSprite();
        CreatePhysics();
    }

    private void CreatePhysics()
    {
        box2DBodyComponent = AddComponent(new Box2DBodyComponent(world.CreateBody(new BodyDef
        {
            type = BodyType.Dynamic,
            position = new Vector2(-5, 16)
        })));

        CircleShape shape = new()
        {
            Radius = 0.8f
        };

        PhysicsBody.CreateFixture(shape, 1.0f);
        PhysicsBody.SetFixedRotation(true);
        PhysicsBody.SetLinearDampling(0.5f);
    }

    private void CreateSprite()
    {
        var sprSheet1 = (new Spritesheet(GameManager.Instance.ContentManager.LoadTexture("content/spritesheet.png"), new Vector2(69, 44)));
        sprSheet1.AddAnimation("idle", new Vector2(0, 0), 6);
        sprSheet1.AddAnimation("run", new Vector2(0, 1), 6);

        Setup(sprSheet1, "idle");
        IsAnimated = true;
        Size = new Vector2(69.0f / 44.0f * 2.0f, 2.0f);
    }

    public override void Update(float dt)
    {
        GenerateTileColliders(dt);
        UpdatePosition(dt);

        base.Update(dt);
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        foreach (var tile in visibleTiles)
            tile.Draw(dt, renderOptions);

        base.Draw(dt, renderOptions);
    }

    private void GenerateTileColliders(float dt)
    {
        // Enumerate the enumerable so its only itterated once.
        visibleTiles = map.FindVisibleTiles(Position + Size / 2.0f, 8.0f).ToArray();

        foreach (var tile in visibleTiles)
        {
            tile.Update(dt);

            if (!colliableTiles.Contains(tile))
            {
                if (tile.TryGenerateCollider())
                {
                    colliableTiles.Add(tile);
                }
            }
        }

        for (int i = colliableTiles.Count - 1; i >= 0; i--)
        {
            var tile = colliableTiles[i];

            tile.PhysicsData.Distance = Vector2.DistanceSquared(tile.GlobalPosition, Position);
            if (tile.PhysicsData.Distance > 25.0f)
            {
                tile.PhysicsData.Age += dt;

                if (tile.PhysicsData.Age > 5.0f)
                {
                    if (tile.TryDestroyCollider())
                    {
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

        PhysicsBody.ApplyForce(movementDir * 100.0f, Transform.Position);
        PhysicsBody.SetLinearVelocity(Vector2.Clamp(PhysicsBody.GetLinearVelocity(), Vector2.One * -5, Vector2.One * 5));

        FrameName = Math.Abs(movementDir.X) > 0 ? "run" : "idle";
        Flipped = movementDir.X < 0;
    }
}