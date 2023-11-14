using Horizon.Core.Content;

namespace Horizon.Core.Primitives
{
    public abstract class GameEngine : Entity, IDisposable
    {
        private bool disposedValue;

        public GameEngine()
        {
            Enabled = true;
            Parent = null!; // base node.

            Engine = this;

            Children = new();
            Components = new();
        }

        public IContentManager Content { get; init; }
        public Silk.NET.OpenGL.GL GL { get; protected set; }

        /// <summary>
        /// Disposes of engine components.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) { }

                Content.Dispose();
                disposedValue = true;
            }
        }

        ~GameEngine()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
