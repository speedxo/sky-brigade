using Horizon.Content.Assets;
using Horizon.Content.Disposers;
using Horizon.Content.Factories;
using Horizon.Core.Components;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content;

public class ContentManager : IGameComponent
{
    private bool disposedValue;

    public AssetManager<Texture, TextureFactory, TextureDescription, TextureDisposer> Textures { get; init; }

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
            //Shaders.Dispose();
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
        
    }

    public void Render(float dt)
    {

    }

    public void UpdateState(float dt)
    {

    }

    public void UpdatePhysics(float dt)
    {

    }
}
