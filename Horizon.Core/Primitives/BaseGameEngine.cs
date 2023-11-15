using Horizon.Core.Content;
using Silk.NET.Windowing;
using static Horizon.Core.Primitives.EngineEventHandler;

namespace Horizon.Core.Primitives;

public class EngineEventHandler
{
    public Action<float>? PreState;
    public Action<float>? PostState;

    public Action<float>? PreRender;
    public Action<float>? PostRender;
}

/// <summary>
/// An abstract class providing implementation providing a GL context and window.
/// </summary>
public abstract class BaseGameEngine : Entity, IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// A public, static reference to the primary GL context held by <see cref="WindowManager"/>
    /// </summary>
    public static Silk.NET.OpenGL.GL GL { get; protected set; }

    /// <summary>
    /// Helper class providing more specific events.
    /// </summary>
    public EngineEventHandler Events { get; init; }

    /// <summary>
    /// Engine component that manages all associated window activities and threads.
    /// </summary>
    public WindowManager WindowManager { get; init; }

    /// <summary>
    /// A copy of the game engines initial configuration.
    /// </summary>
    public GameEngineConfiguration Configuration { get; init; }

    public BaseGameEngine() : this(GameEngineConfiguration.Default) { }
    public BaseGameEngine(in GameEngineConfiguration engineConfiguration)
    {
        Configuration = engineConfiguration;
        Enabled = true;

        Events = new();
        Children = new();
        Components = new();

        // Create window manager, the window manager will bootstrap and call the Render(), UpdateState() and UpdatePhysics()
        WindowManager = AddComponent(new WindowManager(Configuration.WindowConfiguration));
    }

    public override void Initialize()
    {
        base.Initialize();

        // swoop up GL instance
        GL = WindowManager.GL;
    }

    public override void UpdateState(float dt)
    {
        Events.PreState?.Invoke(dt);
        base.UpdateState(dt);
        Events.PostState?.Invoke(dt);
    }

    public override void Render(float dt)
    {
        Events.PreRender?.Invoke(dt);
        base.Render(dt);
        Events.PostRender?.Invoke(dt);
    }

    /// <summary>
    /// Instantiates a window, and opens it.
    /// </summary>
    public virtual void Run() => WindowManager.Run();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                WindowManager.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
