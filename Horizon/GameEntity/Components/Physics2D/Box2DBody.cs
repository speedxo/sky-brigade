using Box2D.NetStandard.Dynamics.Bodies;
using Horizon.Rendering;

namespace Horizon.GameEntity.Components.Physics2D
{
    [RequiresComponent(typeof(TransformComponent2D))]
    public class Box2DBodyComponent : IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }

        private TransformComponent2D transform;

        public Body Body { get; init; }

        public Box2DBodyComponent(Body body)
        {
            this.Body = body;
        }

        public void Initialize()
        {
            transform = Parent.GetComponent<TransformComponent2D>()!;
        }

        public void Render(float dt, ref RenderOptions options) { }

        public void UpdateState(float dt)
        {
            transform.Position = Body.Position;
            transform.Rotation = MathHelper.RadiansToDegrees(Body.GetAngle());
        }

        public void UpdatePhysics(float dt) { }
    }
}
