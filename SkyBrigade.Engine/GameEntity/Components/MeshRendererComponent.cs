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

        public void Draw(float dt, RenderOptions? options = null)
        {
            Mesh?.Draw(dt, options);
        }

        public void Update(float dt)
        {
        }
    }
}