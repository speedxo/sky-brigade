using System.Numerics;
using System.Runtime.InteropServices;
using Bogz.Logging;
using Bogz.Logging.Loggers;
using Horizon.Content.Managers;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Horizon.Engine.Components;
using Horizon.Engine.Framework;
using Horizon.Input;
using Horizon.OpenGL.Managers;

using ImGuiNET;

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

    /// <summary>
    /// Total time in seconds that the window has been open.
    /// </summary>
    public float TotalTime { get; private set; } = 0.0f;

    public EngineEventHandler EventManager { get; init; }
    public ObjectManager ObjectManager { get; init; }
    public WindowManager WindowManager { get; init; }
    public SceneManager SceneManager { get; init; }
    public InputManager InputManager { get; init; }

    private CustomImguiController imguiController;

    public GameEngine(in GameEngineConfiguration engineConfiguration)
    {
        Instance = GameObject.Engine = this;
        Configuration = engineConfiguration;

        Enabled = true;

        // Create window manager, the window manager will bootstrap and call Initialize(), Render(), UpdateState() and UpdatePhysics()
        WindowManager = AddComponent<WindowManager>(new(Configuration.WindowConfiguration));

        // Engine components
        EventManager = AddComponent<EngineEventHandler>();
        ObjectManager = AddComponent<ObjectManager>();
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

        imguiController = new CustomImguiController(GL, WindowManager.Window, WindowManager.Input);
        LoadImGuiStyle();

        SceneManager.AddInstance(Configuration.InitialScene);
    }

    private static void LoadImGuiStyle()
    {
        ImGuiStylePtr style = ImGui.GetStyle();

        style.AntiAliasedLines = true;
        style.AntiAliasedFill = true;
        style.AntiAliasedLinesUseTex = true;

        style.WindowRounding = 5.3f;
        style.FrameRounding = 2.3f;
        style.ScrollbarRounding = 0;

        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.90f, 0.90f, 0.90f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09f, 0.09f, 0.15f, 1.00f);
        style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.05f, 0.05f, 0.10f, 0.85f);
        style.Colors[(int)ImGuiCol.Border] = new Vector4(0.70f, 0.70f, 0.70f, 0.65f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.00f, 0.00f, 0.01f, 1.00f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.90f, 0.80f, 0.80f, 0.40f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.90f, 0.65f, 0.65f, 0.45f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.83f);
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.40f, 0.40f, 0.80f, 0.20f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.00f, 0.00f, 0.00f, 0.87f);
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.01f, 0.01f, 0.02f, 0.80f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.20f, 0.25f, 0.30f, 0.60f);
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.55f, 0.53f, 0.55f, 0.51f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.56f, 0.56f, 0.56f, 1.00f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.91f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.90f, 0.90f, 0.90f, 0.83f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.70f, 0.70f, 0.70f, 0.62f);
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.30f, 0.30f, 0.30f, 0.84f);
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.48f, 0.72f, 0.89f, 0.49f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.50f, 0.69f, 0.99f, 0.68f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.80f, 0.50f, 0.50f, 1.00f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.30f, 0.69f, 1.00f, 0.53f);
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.44f, 0.61f, 0.86f, 1.00f);
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.38f, 0.62f, 0.83f, 1.00f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.00f, 1.00f, 1.00f, 0.85f);
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1.00f, 1.00f, 1.00f, 0.60f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.00f, 1.00f, 1.00f, 0.90f);
        style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.00f, 0.00f, 1.00f, 0.35f);
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
        base.UpdatePhysics(dt);
        base.UpdateState(dt);
        EventManager.PostState?.Invoke(dt);
    }

    public override void Render(float dt, object? obj = null)
    {
        TotalTime += dt;

        // Make sure ImGui is up-to-date before rendering.
        imguiController.Update(dt);


        // Run our custom events.
        EventManager.PreRender?.Invoke(dt);
        base.Render(dt);
        EventManager.PostRender?.Invoke(dt);

        imguiController.Render();
    }

    /// <summary>
    /// Instantiates a window, and opens it.
    /// </summary>
    public virtual void Run() => WindowManager.Run();
}
