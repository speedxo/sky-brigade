using Horizon.Content;
using Horizon.Debugging;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Input;
using Horizon.Logging;
using Horizon.Rendering;
using ImGuiNET;
using ImPlotNET;
using Microsoft.Extensions.Options;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Horizon;

public abstract class GameEngine : Entity
{
    #region Components

    public ContentManager Content { get; protected set; }
    public SkylineDebugger Debugger { get; protected set; }
    public GameScreenManagerComponent GameScreen { get; private set; }
    public InputManager Input { get; private set; }
    public EngineWindowManager Window { get; private set; }
    public Logger Logger { get; private set; }
    public GL GL { get => Window.GL; }

    #endregion
    #region Properties
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
    #endregion
    #region Fields & Members
    private readonly GameInstanceParameters instanceParameters;
    private CustomImguiController _imGuiController;
    private RenderOptions _options;
    #endregion
    #region Event Delegates

    public delegate void PreUpdate(float dt);
    public delegate void PreDraw(float dt);
    public delegate void PostUpdate(float dt);
    public delegate void PostDraw(float dt);

    public event PreDraw? OnPreDraw;    
    public event PreUpdate? OnPreUpdate;    
    public event PostDraw? OnPostDraw;
    public event PostUpdate? OnPostUpdate;


    #endregion
        
    public GameEngine(GameInstanceParameters parameters)
    {
        this.instanceParameters = parameters;
        SetGameEngine(this);

        AddEntity(
            Window = new EngineWindowManager(parameters.InitialWindowSize, parameters.WindowTitle)
        );

        // Register event handlers for the window.
        SubscribeWindowEvents();
    }

    private void SubscribeWindowEvents()
    {
        Window.RenderFrame += WindowDraw;
        Window.UpdateFrame += (delta) =>
        {
            Update((float)delta);
        };
        Window.Closing += DisposeECS;
        Window.Load += Load;
    }

    protected virtual void Load()
    {
        LoadEssentialEngineComponents();

        _options = Debugger.RenderOptionsDebugger.RenderOptions with
        {
            GL = Window.GL
        };


        if (Debugger.Enabled)
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var buildDate = (new DateTime(2000, 1, 1)
                                    .AddDays(version.Build).AddSeconds(version.Revision * 2)).ToString("dd/MM/yyyy");

            Window.UpdateTitle($"{instanceParameters.WindowTitle} - Horizon ({version}) {buildDate}");
        }
    }

    /// <summary>
    /// Helper method to dispose the entity-component system.
    /// </summary>
    private void DisposeECS()
    {
        foreach (var entity in Entities)
        {
            if (entity is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        foreach (var component in Components.Values)
        {
            if (component is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    protected virtual void LoadEssentialEngineComponents()
    {
        // Non ECS
        AssetFactory.SetGameEngine(this);
        Logger = new Logger(LogOutput.Console);
        InitializeImGui();

        // Parameterless Entities
        Content = AddEntity<ContentManager>();
        LoadEssentialAssets();

        Debugger = AddEntity<SkylineDebugger>();

        /// Injected Entities
        Input = AddEntity(new InputManager(Window) {
            CaptureInput = true
        });

        // Components
        GameScreen = AddComponent<GameScreenManagerComponent>();
        // ! We ensure that intialGameScreen has to be of type Scene in ctor
        GameScreen.AddInstance<Scene>((Scene)Activator.CreateInstance(instanceParameters.InitialGameScreen)!);
    }

    private void InitializeImGui()
    {
        _imGuiController = new CustomImguiController(GL, Window.GetWindow(), Window.GetInput());

        // Enable docking for a more streamlined UI
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        //LoadImGuiStyle();
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

    // Method to load essential assets required for the game.
    private void LoadEssentialAssets()
    {
        Content.Shaders.AddNamed(
            "default",
            ShaderFactory.CompileFromDefinitions(
               new ShaderDefinition {
                   Type = ShaderType.VertexShader,
                   Source = @"#version 410 core

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
     }"
               }, new ShaderDefinition {
                   Type = ShaderType.FragmentShader,
                   Source = @"#version 410 core
     out vec4 FinalFragColor;

     in vec2 texCoords;

     uniform sampler2D uAlbedo;

     void main()
     {{
         FinalFragColor = texture(uAlbedo, texCoords);
     }}
     "
               }
            )
        );

        Content.Shaders.AddNamed("basic", ShaderFactory.CompileNamed("Assets/basic_shader/", "basic"));
        Content.Shaders.AddNamed("material_basic", ShaderFactory.CompileNamed("Assets/material_shader/", "basic"));
        Content.Shaders.AddNamed("material_advanced", ShaderFactory.CompileNamed("Assets/material_shader/", "advanced"));

        Content.GenerateNamedTexture("debug", "Assets/among.png");
        Content.GenerateNamedTexture("gray", "Assets/gray.png");
        Content.GenerateNamedTexture("white", "Assets/white.png");
    }
    public void Run() => Window.Run();

    // Variables and method used for non-essential updates that run once per second.
    public override void Update(float dt)
    {
        OnPreUpdate?.Invoke(dt);

        if (Input.WasPressed(VirtualAction.Pause))
        {
            for (int i = 0; i < Input.NativeInputContext.Mice.Count; i++)
                Input.NativeInputContext.Mice[i].Cursor.CursorMode = Input.CaptureInput
                    ? CursorMode.Raw
                    : CursorMode.Normal;
        }

        //oneSecondTimer += dt;
        //if (oneSecondTimer >= 1.0f)
        //{
        //    oneSecondTimer = 0.0f;
        //    nonEssentialUpdate();
        //}

        Debugger.PerformanceDebugger.CpuMetrics.TimeAndTrackMethod(() => {
            base.Update(dt);
        }, "Engine", "CPU");

        OnPostUpdate?.Invoke(dt);
    }

    //// Update method for non-essential tasks, such as measuring memory usage.
    //private void nonEssentialUpdate()
    //{
    //    MemoryUsage = GC.GetTotalMemory(false) / 1000000;
    //}
    private void WindowDraw(double dt) => Draw((float)dt, ref _options);

    public void DrawWithMetrics(in Entity entity, in float dt, ref RenderOptions options)
    {
        var startTime = Stopwatch.GetTimestamp();
        entity.Draw(dt, ref options);
        var endTime = Stopwatch.GetTimestamp();
        
        var val = (double)(endTime - startTime) / Stopwatch.Frequency;
        Engine.Debugger.PerformanceDebugger.GpuMetrics.Aggregate("EngineComponents", entity.Name, val);
    }
    
    public void DrawWithMetrics(in IGameComponent component, in float dt, ref RenderOptions options)
    {
        var startTime = Stopwatch.GetTimestamp();
        component.Draw(dt, ref options);
        var endTime = Stopwatch.GetTimestamp();
        if (component.Name == "Scene Manager") return;

        var val = (double)(endTime - startTime) / Stopwatch.Frequency;
        Engine.Debugger.PerformanceDebugger.GpuMetrics.Aggregate("EngineComponents", component.Name, val);
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        if (!Enabled)
            return;

        var startTime = Stopwatch.GetTimestamp();
        OnPreDraw?.Invoke(dt);

        // Make sure ImGui is up-to-date before rendering.
        _imGuiController.Update(dt);

        // Clear the screen buffer before rendering the game screen.
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render all entities & components
        for (int i = 0; i < Components.Count; i++)
            DrawWithMetrics(Components.Values.ElementAt(i), dt, ref options);

        for (int i = 0; i < Entities.Count; i++)
            DrawWithMetrics(Entities[i], dt, ref options);

        // Render ImGui UI on top of the game screen.
        _imGuiController.Render();
        OnPostDraw?.Invoke(dt);

        // Collect Metrics
        var endTime = Stopwatch.GetTimestamp();
        var elapsedSeconds = (double)(endTime - startTime) / Stopwatch.Frequency;
        Debugger.PerformanceDebugger.GpuMetrics.AddCustom("Engine", "GPU", elapsedSeconds);
    }
}