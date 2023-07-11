using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
    public class MeshRendererComponent : IGameComponent
    {
        public Mesh? Mesh { get; set; }
        public Entity Parent { get; set; }

        public void Initialize()
        {
        }

        public void Draw(RenderOptions? options = null)
        {
            Mesh?.Draw(options);
        }

        public void Update(float dt)
        {
        }
    }
}