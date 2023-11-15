using Horizon.Content.Disposers;
using Horizon.Content.Factories;
using Horizon.Core.Assets;
using Horizon.Core.Components;
using Horizon.Core.Content;
using Horizon.Core.Primitives;

namespace Horizon.Content.Managers;

public class ContentManager : IGameComponent
{
    public AssetManager<Texture, TextureFactory, TextureDescription, TextureDisposer> Textures { get; init; }
    public AssetManager<Shader, ShaderFactory, ShaderDescription, ShaderDisposer> Shaders { get; init; }

    public string Name { get; set; }
    public Entity Parent { get; set; }
    public bool Enabled { get; set; }

    public ContentManager()
    {
        Name = "Content Manager";
        Textures = new();
        Shaders = new();
    }

    public void Initialize()
    {

    }


    public void Dispose()
    {
        Textures.Dispose();
        Shaders.Dispose();
        GC.SuppressFinalize(this);
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
