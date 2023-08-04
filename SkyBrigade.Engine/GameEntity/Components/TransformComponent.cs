using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.GameEntity.Components
{
    public class TransformComponent : IGameComponent
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Front { get => Vector3.Normalize(Rotation); }
        public Entity Parent { get; set; }

        public void Initialize()
        {

        }

        public void Update(float dt)
        {
        }

        public void Draw(float dt, RenderOptions? options = null)
        {
        }
    }
}