using Bogz.Logging;
using Bogz.Logging.Loggers;
using Horizon.Content.Managers;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Factories;
using Horizon.OpenGL.Finalizers;

namespace Horizon.OpenGL.Managers;

/// <summary>
/// Managed class to create, manage and destroy unmanaged OpenGL assets.
/// </summary>
public class ContentManager : IGameComponent, IDisposable
{
    internal static ContentManager Instance { get; private set; }

    public static Silk.NET.OpenGL.GL GL { get; private set; }

    public AssetManager<
        Texture,
        TextureFactory,
        TextureDescription,
        TextureFinalizer
    > Textures { get; init; }

    public AssetManager<
        Shader,
        ShaderFactory,
        ShaderDescription,
        ShaderFinalizer
    > Shaders { get; init; }

    public AssetManager<
        BufferObject,
        BufferObjectFactory,
        BufferObjectDescription,
        BufferObjectFinalizer
    > Buffers { get; init; }

    public AssetManager<
        FrameBufferObject,
        FrameBufferObjectFactory,
        FrameBufferObjectDescription,
        FrameBufferObjectFinalizer
    > FrameBuffers { get; init; }

    public AssetManager<
        VertexArrayObject,
        VertexArrayObjectFactory,
        VertexArrayObjectDescription,
        VertexArrayObjectFinalizer
    > VertexArrays { get; init; }

    public string Name { get; set; }
    public Entity Parent { get; set; }
    public bool Enabled { get; set; }

    public ContentManager()
    {
        Instance = this;
        Name = "Content Manager";

        // primitive types
        Textures = new();
        Shaders = new();
        Buffers = new();
        FrameBuffers = new();
        VertexArrays = new();
    }

    public void Initialize()
    {
        GL = Parent.GetComponent<WindowManager>().GL;

        Textures.SetMessageCallback(ConcurrentLogger.Instance.Log);
        Shaders.SetMessageCallback(ConcurrentLogger.Instance.Log);
        Buffers.SetMessageCallback(ConcurrentLogger.Instance.Log);
        VertexArrays.SetMessageCallback(ConcurrentLogger.Instance.Log);
        FrameBuffers.SetMessageCallback(ConcurrentLogger.Instance.Log);
    }

    public void Render(float dt, object? obj = null) { }

    public void UpdateState(float dt) { }

    public void UpdatePhysics(float dt) { }

    public void Dispose()
    {
        // textures bound to fbo so we free the fbos first.
        FrameBuffers.Dispose();
        Textures.Dispose();

        Shaders.Dispose();

        // the vbos are bound to vaos so we need to dispose the vaos first.
        VertexArrays.Dispose();
        Buffers.Dispose();

        GC.SuppressFinalize(this);
    }
}
