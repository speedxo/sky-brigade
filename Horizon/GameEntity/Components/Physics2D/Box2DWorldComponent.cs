using System;
using Box2D.NetStandard.Dynamics.Bodies;
using Horizon.Rendering;

namespace Horizon.GameEntity.Components.Physics2D;

public class Box2DWorldComponent : Box2D.NetStandard.Dynamics.World.World, IGameComponent
{
    public string Name { get; set; }
    public Entity Parent { get; set; }

    public Box2DWorldComponent()
        :base(new System.Numerics.Vector2(0, -9.8f))
    {

    }

    public void Initialize()
    {

    }

    public void Update(float dt)
    {
        Step(dt, 8, 3);
    }


    public void Draw(float dt, RenderOptions? options = null)
    {

    }
}

