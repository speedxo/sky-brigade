using Horizon.GameEntity;
using Horizon.GameEntity.Components;

namespace Horizon.Rendering.Shapes
{
    public class Terrain : Entity
    {
        public TransformComponent Transform { get; init; }
        public MeshRendererComponent MeshComponent { get; init; }

        public Material Material => MeshComponent.Material;

        public Terrain()
        {
            Transform = AddComponent<TransformComponent>();
            MeshComponent = AddComponent<MeshRendererComponent>();
        }

        public void GenerateTerrain(int seed)
        {
        }
    }
}