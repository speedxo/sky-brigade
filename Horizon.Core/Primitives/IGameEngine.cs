using Horizon.Core.Content;

namespace Horizon.Core.Primitives
{
    public abstract class GameEngine : Entity, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// The current IGameEngine instance.
        /// </summary>
        public static GameEngine Current { get; protected set; }

        public GameEngine()
        {
            Enabled = true;
            Parent = null!; // base node.

            Current = this;
            Engine = this;

            Children = new();
            Components = new();
        }

        public IContentManager Content { get; init; }

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
