using System.Runtime.InteropServices;
using Bogz.Logging;
using Bogz.Logging.Loggers;
using Horizon.Content.Managers;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Horizon.Engine.Components;
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

    public GL GL
    {
        get => WindowManager.GL;
    }

    public static GameEngine Instance { get; private set; }

    /// <summary>
    /// Gets the main active camera associated with the current active Scene.
    /// </summary>
    public Camera? ActiveCamera
    {
        get => SceneManager.CurrentInstance?.ActiveCamera;
    }

    public EngineEventHandler EventManager { get; init; }
    public ContentManager ContentManager { get; init; }
    public WindowManager WindowManager { get; init; }
    public SceneManager SceneManager { get; init; }
    public InputManager InputManager { get; init; }

    public GameEngine(in GameEngineConfiguration engineConfiguration)
    {
        Instance = GameObject.Engine = this;
        Configuration = engineConfiguration;

        Enabled = true;

        // Create window manager, the window manager will bootstrap and call Initialize(), Render(), UpdateState() and UpdatePhysics()
        WindowManager = AddComponent<WindowManager>(new(Configuration.WindowConfiguration));

        // Engine components
        EventManager = AddComponent<EngineEventHandler>();
        ContentManager = AddComponent<ContentManager>();
        InputManager = AddComponent<InputManager>();
        SceneManager = AddComponent<SceneManager>();
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

        SceneManager.AddInstance(Configuration.InitialScene);
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

        ConcurrentLogger
            .Instance
            .Log(
                LogLevel.Info,
                $"[{source}] [{severity}] [{type}] [{id}] {Marshal.PtrToStringAnsi(message)}"
            );
    }

    public override void UpdateState(float dt)
    {
        // Run our custom events.
        EventManager.PreState?.Invoke(dt);
        base.UpdateState(dt);
        EventManager.PostState?.Invoke(dt);
    }

    public override void Render(float dt, object? obj = null)
    {
        // Run our custom events.
        EventManager.PreRender?.Invoke(dt);
        base.Render(dt);
        EventManager.PostRender?.Invoke(dt);
    }

    /// <summary>
    /// Instantiates a window, and opens it.
    /// </summary>
    public virtual void Run() => WindowManager.Run();
}
