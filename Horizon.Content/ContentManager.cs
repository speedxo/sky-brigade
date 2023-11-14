using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content
{
    public class ContentManager : IContentManager
    {
        private bool disposedValue;

        public IAssetManager<IGameAsset> Textures { get; init; }
        public IAssetManager<IGameAsset> Shaders { get; init; }

        public string Name { get; set; }
        public Entity Parent { get; set; }
        public bool Enabled { get; set; }

        public ContentManager(Entity parent)
        {
            Name = "Content Manager";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // managed
                }

                Textures.Dispose();
                Shaders.Dispose();
                disposedValue = true;
            }
        }

        ~ContentManager()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Render(float dt)
        {
            throw new NotImplementedException();
        }

        public void UpdateState(float dt)
        {
            throw new NotImplementedException();
        }

        public void UpdatePhysics(float dt)
        {
            throw new NotImplementedException();
        }
    }
}
