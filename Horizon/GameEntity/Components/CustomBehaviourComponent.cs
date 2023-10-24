using Horizon.Rendering;

namespace Horizon.GameEntity.Components
{
    public class CustomBehaviourComponent : IGameComponent
    {
        public string Name { get; set; }

        public Entity Parent { get; set; }

        public delegate void OnUpdateDelegate(float dt);

        public delegate void OnDrawDelegate(float dt, ref RenderOptions options);

        public event OnDrawDelegate? OnDraw;

        public event OnUpdateDelegate? OnUpdate;

        public void Initialize() { }

        public void Update(float dt)
        {
            OnUpdate?.Invoke(dt);
        }

        public void Draw(float dt, ref RenderOptions options)
        {
            OnDraw?.Invoke(dt, ref options);
        }
    }
}
