using Horizon.Content;
using Horizon.Debugging;
using Horizon.GameEntity;
using Horizon.Input;
using Horizon.Logging;
using Horizon.OpenGL;
using Horizon.Rendering;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.InteropServices;

// Namespace declaration for the GameManager class
namespace Horizon;

/// <summary>
/// The GameManager class manages the main game loop and essential components for a game.
/// </summary>
public class GameManager : Entity, IDisposable
{
    // Singleton pattern: Lazy initialization of a single instance of the GameManager class
    private static readonly Lazy<GameManager> _instance = new Lazy<GameManager>(
        () => new GameManager()
    );

    /// <summary>
    /// Gets the singleton instance of the GameManager class.
    /// </summary>
    public static GameManager Instance => _instance.Value;

    #region Public Properties

    /// <summary>
    /// Gets the ContentManager responsible for loading and managing game assets.
    /// </summary>
    public ContentManager ContentManager { get; private set; }

    /// <summary>
    /// The screen aspect ratio (w/h)
    /// </summary>
    public float AspectRatio { get; private set; }

    /// <summary>
    /// Gets the unified debugger class for debugging game elements.
    /// </summary>
    public SkylineDebugger Debugger { get; private set; }

    /// <summary>
    /// The viewport size.
    /// </summary>
    public Vector2 ViewportSize { get; private set; }

    /// <summary>
    /// The window size.
    /// </summary>
    public Vector2 WindowSize { get; private set; }

    /// <summary>
    /// Gets the OpenGL context used for rendering.
    /// </summary>
    public GL Gl { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the input is captured (e.g., for controlling the game).
    /// </summary>
    public bool IsInputCaptured { get; private set; } = true;

    /// <summary>
    /// Gets the GameScreenManager responsible for managing different game screens or scenes.
    /// </summary>
    public GameScreenManager GameScreenManager { get; private set; }

    /// <summary>
    /// Gets the input context used for handling user input.
    /// </summary>
    public IInputContext Input { get; private set; }

    /// <summary>
    /// Gets the new and improved unified input manager.
    /// </summary>
    public InputManager InputManager { get; private set; }

    /// <summary>
    /// Gets the total memory usage of the application in megabytes (MB).
    /// </summary>
    public long MemoryUsage { get; private set; }

    /// <summary>
    /// Gets the main game window.
    /// </summary>
    public IWindow Window { get; private set; }

    /// <summary>
    /// Gets the logger used for logging application messages.
    /// </summary>
    public Logger Logger { get; private set; }

    /// <summary>
    /// The current operating system information.
    /// </summary>
    public static class OperatingSystem
    {
        static OperatingSystem()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static bool IsWindows { get; }

        public static bool IsMacOS { get; }

        public static bool IsLinux { get; }
    }

    #endregion Public Properties

    #region Private Properties

    private ImGuiController imguiController;
    private Type initialGameScreen;
    private float oneSecondTimer;

    #endregion Private Properties

    /// <summary>
    /// Runs the game with the specified parameters and sets up the main game loop.
    /// </summary>
    /// <param name="parameters">Parameters used to initialize the game instance.</param>
    /// <returns>The initialized GameManager instance.</returns>
    public GameManager Initialize(GameInstanceParameters parameters)
    {
        // Store the initial game screen that we should display when the game starts.
        this.initialGameScreen = parameters.InitialGameScreen;
        if (!typeof(Scene).IsAssignableFrom(this.initialGameScreen))
            Logger.Log(LogLevel.Fatal, "Initial game screen is not of type Scene!");

        // Create a window with the specified options.
        var options = WindowOptions.Default with
        {
            API = new GraphicsAPI()
            {
                Flags = ContextFlags.ForwardCompatible,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                Version = new APIVersion(4, 1)
            },
            Title = parameters.WindowTitle,
            Size = new Silk.NET.Maths.Vector2D<int>(
                (int)parameters.InitialWindowSize.X,
                (int)parameters.InitialWindowSize.Y
            ),
            FramesPerSecond = 0,
            VSync = false
        };

        // Create the window.
        this.Window = Silk.NET.Windowing.Window.Create(options);

        // Register event handlers for the window.
        Window.Render += (delta) =>
        {
            Draw((float)delta, Debugger.RenderOptionsDebugger.RenderOptions with { GL = Gl });
        };
        Window.Update += (delta) =>
        {
            Update((float)delta);
        };
        Window.Load += onLoad;
        Window.Closing += Window_Closing;
        Window.Resize += Window_Resize;

        return this;
    }

    private void Window_Resize(Silk.NET.Maths.Vector2D<int> obj)
    {
        FrameBufferManager.ResizeAll((int)ViewportSize.X, (int)ViewportSize.Y);
    }

    public void Run()
    {
        // Run the window.
        Window.Run();
        Window.Dispose();
    }

    private void Window_Closing()
    {
        Dispose();
    }

    // Method called when the game is loaded, before the game loop starts.
    private void onLoad()
    {
        // Initialize ImGui controller for UI rendering.
        imguiController = new ImGuiController(
            Gl = Window.CreateOpenGL(), // Load OpenGL
            Window, // Pass in our window
            Input = Window.CreateInput() // Create an input context
        );

        // Enable docking for a more streamlined UI
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        // Setup custom ImGui styles
        LoadImGuiStyle();

        // Initialize the logger with both file and console outputs.
        Logger = new Logger(LogOutput.Console);

        // Check if at least one keyboard is available for input.
        if (Input.Keyboards.Count < 1)
            throw new Exception(
                "Cannot play without a keyboard. A keyboard is required for this game to function."
            );

        // initialize and add the input manager
        InputManager = AddEntity<InputManager>();

        // Initialize the ContentManager responsible for loading assets.
        ContentManager = AddEntity<ContentManager>();
        LoadEssentialAssets();

        // Load the debugger.
        Debugger = AddEntity<SkylineDebugger>();

        UpdateViewport();

        // Initialize the GameScreenManager and set the initial game screen.
        GameScreenManager = AddEntity<GameScreenManager>();
        // ! We ensure that intialGameScreen has to be of type Scene in ctor
        GameScreenManager.AddInstance<Scene>((Scene)Activator.CreateInstance(initialGameScreen)!);

        Gl.Enable(EnableCap.VertexArray);
        for (int i = 0; i < Input.Mice.Count; i++)
            Input.Mice[i].Cursor.CursorMode = IsInputCaptured ? CursorMode.Raw : CursorMode.Normal;
    }

    private void UpdateViewport()
    {
        WindowSize = new Vector2(Window.FramebufferSize.X, Window.FramebufferSize.Y);
        ViewportSize =
            (Debugger.Enabled && Debugger.GameContainerDebugger.Visible)
                ? new Vector2(
                    Debugger.GameContainerDebugger.FrameBuffer.Width,
                    Debugger.GameContainerDebugger.FrameBuffer.Height
                )
                : (new Vector2(Window.FramebufferSize.X, Window.FramebufferSize.Y));
        AspectRatio = WindowSize.X / WindowSize.Y;
    }

    private static void LoadImGuiStyle()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
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

    // Method to load essential assets required for the game.
    private void LoadEssentialAssets()
    {
        ContentManager.GenerateNamedShader(
            "default",
            OpenGL.Shader.CompileShaderFromSource(
                @"#version 410 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 2) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uModel;

out vec2 texCoords;

void main()
{
    texCoords = vTexCoords;

    // Trying to understand the universe through vertex manipulation!
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
}",
                @"#version 410 core
out vec4 FinalFragColor;

in vec2 texCoords;

uniform sampler2D uAlbedo;

void main()
{{
    FinalFragColor = texture(uAlbedo, texCoords);
}}
"
            )
        );
        ContentManager.GenerateNamedShader(
            "material_basic",
            "Assets/material_shader/basic.vert",
            "Assets/material_shader/basic.frag"
        );
        ContentManager.GenerateNamedShader(
            "basic",
            "Assets/basic_shader/basic.vert",
            "Assets/basic_shader/basic.frag"
        );
        ContentManager.GenerateNamedShader(
            "material_advanced",
            "Assets/material_shader/advanced.vert",
            "Assets/material_shader/advanced.frag"
        );

        ContentManager.GenerateNamedTexture("debug", "Assets/among.png");
        ContentManager.GenerateNamedTexture("gray", "Assets/gray.png");
        ContentManager.GenerateNamedTexture("white", "Assets/white.png");
    }

    // Variables and method used for non-essential updates that run once per second.
    public override void Update(float dt)
    {
        Debugger.PerformanceDebugger.UpdateStart(dt);

        if (InputManager.WasPressed(VirtualAction.Pause))
        {
            IsInputCaptured = !IsInputCaptured;
            for (int i = 0; i < Input.Mice.Count; i++)
                Input.Mice[i].Cursor.CursorMode = IsInputCaptured
                    ? CursorMode.Raw
                    : CursorMode.Normal;
        }

        oneSecondTimer += dt;
        if (oneSecondTimer >= 1.0f)
        {
            oneSecondTimer = 0.0f;
            nonEssentialUpdate();
        }

        UpdateViewport();

        base.Update(dt);
        Debugger.PerformanceDebugger.UpdateEnd();
    }

    // Update method for non-essential tasks, such as measuring memory usage.
    private void nonEssentialUpdate()
    {
        MemoryUsage = GC.GetTotalMemory(false) / 1000000;
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        Debugger.PerformanceDebugger.RenderStart(dt);

        // Make sure ImGui is up-to-date before rendering.
        imguiController.Update(dt);

        // Clear the screen buffer before rendering the game screen.
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render all entities
        base.Draw(dt, renderOptions);

        // Render ImGui UI on top of the game screen.
        imguiController.Render();
        Debugger.PerformanceDebugger.RenderEnd();
    }

    public void Dispose()
    {
        ContentManager.Dispose();
        GameScreenManager.Dispose();

        Logger.Dispose();
    }
}
