using System.Runtime.InteropServices;
using Bogz.Logging;
using Bogz.Logging.Loggers;
using Horizon.Content.Managers;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Horizon.Input;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;

namespace Horizon.Engine;

public class GameEngine : Entity
{
    /// <summary>
    /// A copy of the game engines initial configuration.
    /// </summary>
    public GameEngineConfiguration Configuration { get; init; }

    public Silk.NET.OpenGL.GL GL
    {
        get => Window.GL;
    }

    /// <summary>
    /// Temporary solution to context sharing while i work on the rewrite.
    /// </summary>
    public static GameEngine Instance { get; private set; }

    public ContentManager Content { get; init; }

    public InputManager Input { get; init; }

    public EngineEventHandler Events { get; init; }
    public LoggerComponent Logger { get; init; }
    public WindowManager Window { get; init; }

    public GameEngine(in GameEngineConfiguration engineConfiguration)
    {
        Instance = this;
        Configuration = engineConfiguration;

        Enabled = true;

        // Create window manager, the window manager will bootstrap and call Initialize(), Render(), UpdateState() and UpdatePhysics()
        Window = AddComponent<WindowManager>(new(Configuration.WindowConfiguration));

        // Engine components
        Events = AddComponent<EngineEventHandler>();
        Content = AddComponent<ContentManager>();
        Logger = AddComponent<LoggerComponent>();
        Input = AddComponent<InputManager>();
    }

    public override void Initialize()
    {
        base.Initialize();

        unsafe
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(debugCallback, null);
        }
    }

    private void debugCallback(
        GLEnum source,
        GLEnum type,
        int id,
        GLEnum severity,
        int length,
        nint message,
        nint userParam
    )
    {
        if (id == 131185 || id == 1280)
            return;

        Logger.Log(
            LogLevel.Info,
            $"[{source}] [{severity}] [{type}] [{id}] {Marshal.PtrToStringAnsi(message)}"
        );
    }

    public override void UpdateState(float dt)
    {
        // Run our custom events.
        Events.PreState?.Invoke(dt);
        base.UpdateState(dt);
        Events.PostState?.Invoke(dt);
    }

    public override void Render(float dt)
    {
        // Run our custom events.
        Events.PreRender?.Invoke(dt);
        base.Render(dt);
        Events.PostRender?.Invoke(dt);
    }

    /// <summary>
    /// Instantiates a window, and opens it.
    /// </summary>
    public virtual void Run() => Window.Run();
}
