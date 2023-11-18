using Horizon.Core;
using Horizon.Core.Components;

namespace Horizon.GameEntity.Components.Physics2D;

public class Box2DWorldComponent : Box2D.NetStandard.Dynamics.World.World, IGameComponent
{
    public string Name { get; set; }
    public Entity Parent { get; set; }
    public bool Enabled { get; set; }

    public Box2DWorldComponent()
        : base(System.Numerics.Vector2.Zero) { }

    public void Initialize() { }

    public void UpdateState(float dt)
    {
        Step(dt, 8, 3);
    }

    public void Render(float dt, object? obj = null) { }

    public void UpdatePhysics(float dt) { }
}
