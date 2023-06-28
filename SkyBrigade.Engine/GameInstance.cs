using System;
using System.Diagnostics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace SkyBrigade.Engine;

public class GameInstance : IDisposable
{
    private static readonly Lazy<GameInstance> _instance = new Lazy<GameInstance>(() => new GameInstance());

    public static GameInstance Instance => _instance.Value;

    #region Public Members
    public GL Gl { get; private set; }
    public bool IsInputCaptured { get; private set; } = true;
    public GameScreenManager GameScreenManager { get; private set; }
    public IInputContext Input { get; private set; }
    public long MemoryUsage { get; private set; }
    public IWindow Window { get; private set; }
    #endregion

    #region Private Properties
    private ImGuiController imguiController;
    private Type initialGameScreen;
    #endregion

    public void Run(GameInstanceParameters parameters)
    {
        this.initialGameScreen = parameters.InitialGameScreen;

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
            Size = new Silk.NET.Maths.Vector2D<int>((int)parameters.InitialWindowSize.X, (int)parameters.InitialWindowSize.Y)
        };

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Render += onRender;
        Window.Update += onUpdate;
        Window.Load += onLoad;

        Window.Run();
    }

    private void onLoad()
    {
        imguiController = new ImGuiController(
            Gl = Window.CreateOpenGL(), // load OpenGL
            Window, // pass in our window
            Input = Window.CreateInput() // create an input context
        );

        if (Input.Keyboards.Count < 1)
            throw new Exception("No ways youre actually tryna play without a keyboard\n for any of this shitty code to work you *need* a keyboard.");

        for (int i = 0; i < Input.Keyboards.Count; i++)
        {
            Input.Keyboards[i].KeyDown += onKeyDown;
        }


        GameScreenManager = new GameScreenManager(Gl);
        GameScreenManager.ChangeGameScreen(initialGameScreen);

    }

    private void onKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            IsInputCaptured = !IsInputCaptured;
            for (int i = 0; i < Input.Mice.Count; i++)
                Input.Mice[i].Cursor.CursorMode = IsInputCaptured ? CursorMode.Raw : CursorMode.Normal;
        }

        if (key == Key.Q) Window.Close();
    }


    private float oneSecondTimer = 0.0f;
    private void onUpdate(double delta)
    {
        oneSecondTimer += (float)delta;
        if (oneSecondTimer >= 1.0f)
        {
            oneSecondTimer = 0.0f;
            nonEssentialUpdate();
        }

        GameScreenManager.Update((float)delta);
    }


    private void nonEssentialUpdate()
    {
        MemoryUsage = GC.GetTotalMemory(false) / 1000000;
    }

    private unsafe void onRender(double delta)
    {
        // Make sure ImGui is up-to-date
        imguiController.Update((float)delta);

        // Clearing the screen buffer
        Gl.Clear(ClearBufferMask.ColorBufferBit);


        GameScreenManager.Render(Gl, (float)delta);

        // Make sure ImGui renders too!
        imguiController.Render();
    }

    public void Dispose()
    {
        Window.Dispose();
    }
}

