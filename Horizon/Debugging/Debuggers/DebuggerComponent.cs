using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Logging;
using Horizon.Rendering;

namespace Horizon.Debugging.Debuggers
{
    public abstract class DebuggerComponent : IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }

        public bool Visible = false;

        public abstract void Initialize();

        public abstract void Update(float dt);

        public abstract void Draw(float dt, RenderOptions? options = null);

        protected void Log(LogLevel level, string msg) =>
            GameManager.Instance.Logger.Log(level, $"({Name}) {msg}");
    }
}
