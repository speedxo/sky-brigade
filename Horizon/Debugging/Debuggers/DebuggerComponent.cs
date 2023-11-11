using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Logging;
using Horizon.Rendering;

namespace Horizon.Debugging.Debuggers
{
    public abstract class DebuggerComponent : IGameComponent, IDisposable
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }

        public bool Visible = false;

        public abstract void Initialize();

        public abstract void UpdateState(float dt);

        public abstract void Render(float dt, ref RenderOptions options);

        protected void Log(LogLevel level, string msg)
        {
            Entity.Engine.Logger.Log(level, $"({Name}) {msg}");
        }

        public abstract void UpdatePhysics(float dt);

        public abstract void Dispose();
    }
}
