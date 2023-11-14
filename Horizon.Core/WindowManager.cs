using Horizon.Core.Primitives;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core;


public class WindowManager : Entity
{
    private IWindow _window,
        _updateWindow;
    private IInputContext _input;
    private Task _updateTask, _physicsTask;

    public Action<double> UpdateStateFrame;
    public Action<double> UpdatePhysicsFrame;
    public Action<double> RenderFrame;

    public Action Closing;
    public Action Load;

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
    /// 
    /// </summary>
    public GL GL { get; private set; }

    /// <summary>
    /// Gets the underlying native window.
    /// </summary>
    /// <returns>The GLFW IWindow.</returns>
    public IWindow GetWindow() => _window;

    /// <summary>
    /// Gets the windows native input context.
    /// </summary>
    /// <returns>Native IInputContext</returns>
    public IInputContext GetInput() => _input;


    // copy of initial WindowOptions instance.
    private readonly WindowOptions _options;

    public WindowManager(in Vector2 windowSize, in string title)
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
            Title = title,
            Size = new Silk.NET.Maths.Vector2D<int>((int)windowSize.X, (int)windowSize.Y),
            FramesPerSecond = 0,
            VSync = false
        };

        ViewportSize = WindowSize = windowSize;

        // Create the window.
        this._window = Window.Create(_options);
        SubscribeWindowEvents();
    }

    private void SubscribeWindowEvents()
    {
        this._window.Render += (dt) =>
        {
            RenderFrame?.Invoke(dt);
        };
        this._window.Update += (dt) =>
        {
            UpdateStateFrame?.Invoke(dt);
        };
        this._window.Resize += WindowResize;

        this._window.Load += () =>
        {
            GL = _window.CreateOpenGL();
            _input = _window.CreateInput();

            Load?.Invoke();
            UpdateViewport();
        };
        this._window.Closing += () => Closing?.Invoke();
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

    public void Close()
    {
        if (IsRunning)
            _window.Close();
        else
            throw new Exception("Cannot stop a window that never ran in the first palce!");
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
                UpdatePhysicsFrame?.Invoke(elapsedTime);

            previousTicks = ticks;
        }
    }

    private void OnFrame()
    {
        _window.DoEvents();

        if (!_window.IsClosing)
            _window.DoRender();

        // Dispatch threads.
        _updateTask ??= Task.Run(OnLogicFrame);
        _physicsTask ??= Task.Run(OnPhysicsFrame);
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