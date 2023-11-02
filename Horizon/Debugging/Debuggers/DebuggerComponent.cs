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

        public abstract void Update(float dt);

        public abstract void Draw(float dt, ref RenderOptions options);

        protected void Log(LogLevel level, string msg)
        {
            Entity.Engine.Logger.Log(level, $"({Name}) {msg}");
        }

        public abstract void Dispose();
    }
}
