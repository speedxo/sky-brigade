using Horizon.Core.Content;

namespace Horizon.Core.Primitives
{
    public abstract class GameEngine : Entity, IDisposable
    {
        /// <summary>
        /// A public, static reference to the primary GL context held by <see cref="WindowManager"/>
        /// </summary>
        public static Silk.NET.OpenGL.GL GL { get; protected set; }

        public WindowManager WindowManager { get; init; }

        private bool disposedValue;

        public GameEngine()
        {
            Enabled = true;
            Parent = null!; // base node.
            Children = new();
            Components = new();

            WindowManager = AddEntity(new WindowManager(new System.Numerics.Vector2(1600, 900), "pog u dud"));
        }

        public virtual void Run() => WindowManager.Run();

        public virtual void Dispose()
        {

        }
    }
}
