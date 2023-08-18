using Horizon.Data;
using Horizon.Rendering;
using System.Numerics;

namespace Horizon.GameEntity.Components
{
    [RequiresComponent(typeof(TransformComponent))]
    public class MeshRendererComponent : Mesh, IGameComponent
    {
        public string Name { get; set; }

        public Entity Parent { get; set; }
        public TransformComponent Transform { get; private set; }

        public void Initialize()
        {
            Transform = Parent.GetComponent<TransformComponent>();
        }

        public override void Use(RenderOptions options)
        {
            base.Use(options);
            SetUniform("uModel", Transform.ModelMatrix);
        }

        public void Update(float dt)
        {
        }
    }
}