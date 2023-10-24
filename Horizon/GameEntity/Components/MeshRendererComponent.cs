using Horizon.Rendering;

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
            Transform = Parent.GetComponent<TransformComponent>()!;
            // ! We attached a [RequiresComponent(typeof(TransformComponent))]
        }

        public override void Use(ref RenderOptions options)
        {
            base.Use(ref options);
            SetUniform("uModel", Transform.ModelMatrix);
        }

        public void Update(float dt) { }
    }
}
