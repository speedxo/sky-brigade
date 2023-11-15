using Horizon.Core.Primitives;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Components;

namespace Horizon.Core;

/// <summary>
/// BaseGameEngine component to manage a window and its associated threads.
/// </summary>
public class WindowManager : IGameComponent
{
    private readonly IWindow _window;
    private IInputContext _input;

    public bool IsRunning { get; private set; }

    /// <summary>
    /// The screen aspect ratio (w/h)
    /// </summary>
    public float AspectRatio { get; private set; }

    /// <summary>
    /// The viewport size.
    /// </summary>
    public Vector2 ViewportSize { get; private set; }

    /// <summary>
    /// The window size.
    /// </summary>
    public Vector2 WindowSize { get; private set; }

    /// <summary>
    /// The GL context associated with the windows main render thread.
    /// </summary>
    public GL GL { get; private set; }

    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }

    private BaseGameEngine _engine;

    /// <summary>
    /// Gets the underlying native window.
    /// </summary>
    /// <returns>The GLFW IWindow.</returns>
    public IWindow Window { get => _window; }

    /// <summary>
    /// Gets the windows native input context.
    /// </summary>
    /// <returns>Native IInputContext</returns>
    public IInputContext Input { get => _input; }

    // copy of initial WindowOptions instance.
    private readonly WindowOptions _options;

    public WindowManager(in WindowManagerConfiguration config)
    {
        // Create a window with the specified options.
        _options = WindowOptions.Default with
        {
            API = new GraphicsAPI()
            {
                Flags = ContextFlags.Debug,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                Version = new APIVersion(4, 6)
            },
            Title = config.WindowTitle,
            Size = new Silk.NET.Maths.Vector2D<int>((int)config.WindowSize.X, (int)config.WindowSize.Y),
            FramesPerSecond = 0,
            VSync = false
        };

        ViewportSize = WindowSize = config.WindowSize;

        // Create the window.
        this._window = Silk.NET.Windowing.Window.Create(_options);
        SubscribeWindowEvents();
    }

    private void SubscribeWindowEvents()
    {
        this._window.Render += (dt) => _engine.Render((float)dt);
        this._window.Update += (dt) => _engine.UpdateState((float)dt);
        this._window.Resize += WindowResize;
        
        this._window.Load += Initialize;
    }

    private void UpdateViewport()
    {
        WindowSize = new Vector2(_window.FramebufferSize.X, _window.FramebufferSize.Y);
        ViewportSize = new Vector2(_window.FramebufferSize.X, _window.FramebufferSize.Y);
        AspectRatio = WindowSize.X / WindowSize.Y;
    }

    private void WindowResize(Silk.NET.Maths.Vector2D<int> size)
    {
        //FrameBufferManager.ResizeAll(size.X, size.Y);
        UpdateViewport();
    }

    public void Initialize()
    {
        GL = _window.CreateOpenGL();
        _input = _window.CreateInput();
        _engine = (BaseGameEngine)Parent;


        UpdateViewport();
        _engine.Initialize();
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

    public void Run()
    {
        if (IsRunning)
            throw new Exception("Window is already running!");

        IsRunning = true;

        // Create the window.
        _window.Initialize();

        // Run the loop.
        _window.Run(OnFrame);

        // Dispose and unload
        _window.DoEvents();
        _window.Reset();
    }

    private void OnLogicFrame()
    {
        while (!_window.IsClosing)
        {
            if (_window.IsInitialized)
                _window.DoUpdate();
        }
    }

    private void OnPhysicsFrame()
    {
        long previousTicks = 0,
            ticks;
        double elapsedTime;
        while (!_window.IsClosing)
        {
            ticks = Environment.TickCount64;
            elapsedTime = ((previousTicks - ticks) / (double)Stopwatch.Frequency);

            if (_window.IsInitialized)
                UpdatePhysics((float)elapsedTime);

            previousTicks = ticks;
        }
    }

    private void OnFrame()
    {
        _window.DoEvents();

        if (!_window.IsClosing)
            _window.DoRender();

        /* it is important to ensure that atleast one Render pass has happened, before
         * we dispatch all the threads, as lazy initialisation is done in the render thread. */

        // Dispatch threads.
        Task.Run(OnLogicFrame);
        Task.Run(OnPhysicsFrame);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _window.Dispose();
    }

    /// <summary>
    /// Updates the windows title to the specified string <paramref name="title"/>.
    /// </summary>
    /// <param name="title">The new window title.</param>
    public void UpdateTitle(string title)
    {
        _window.Title = title;
    }
}