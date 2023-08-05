using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.GameEntity.Components
{
    [RequiresComponent(typeof(TransformComponent))]
    public class MeshRendererComponent : Mesh, IGameComponent
    {
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