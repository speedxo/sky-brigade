using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
    public class MeshRendererComponent : Mesh, IGameComponent
    {
        public MeshRendererComponent(Func<(ReadOnlyMemory<Vertex>, ReadOnlyMemory<uint>)> loader, Material? mat = null) : base(loader, mat)
        {

        }

        public Entity Parent { get; set; }

        public void Initialize()
        {

        }

        public void Update(float dt)
        {
        }
    }
}