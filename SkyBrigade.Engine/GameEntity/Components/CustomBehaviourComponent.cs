using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
    public class CustomBehaviourComponent : IGameComponent
    {
        public Entity Parent { get; set; }

        public delegate void OnUpdateDelegate(float dt);

        public delegate void OnDrawDelegate(float dt, RenderOptions? options = null);

        public event OnDrawDelegate? OnDraw;

        public event OnUpdateDelegate? OnUpdate;

        public void Initialize()
        {
        }

        public void Update(float dt)
        {
            OnUpdate?.Invoke(dt);
        }

        public void Draw(float dt, RenderOptions? options = null)
        {
            OnDraw?.Invoke(dt, options);
        }
    }
}