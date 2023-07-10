﻿using System;
using System.Diagnostics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SkyBrigade.Engine.Content;
using SkyBrigade.Engine.Logging;

namespace SkyBrigade.Engine;

public class GameManager : IDisposable
{
    private static readonly Lazy<GameManager> _instance = new Lazy<GameManager>(() => new GameManager());

    public static GameManager Instance => _instance.Value;

    #region Public Members
    public ContentManager ContentManager { get; private set; }
    public GL Gl { get; private set; }
    public bool IsInputCaptured { get; private set; } = true;
    public GameScreenManager GameScreenManager { get; private set; }
    public IInputContext Input { get; private set; }
    public long MemoryUsage { get; private set; }
    public IWindow Window { get; private set; }
    public Logger Logger { get; private set; }
    #endregion

    #region Private Properties
    private ImGuiController imguiController;
    private Type initialGameScreen;
    #endregion

    public void Run(GameInstanceParameters parameters)
    {
        // Store the initial game screen that we should display when the game starts.
        this.initialGameScreen = parameters.InitialGameScreen;

        // Create a window with the specified options.
        // This window is used to display the game to the user.
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

        // Create the window.
        Window = Silk.NET.Windowing.Window.Create(options);

        // Register event handlers for the window.
        Window.Render += onRender;
        Window.Update += onUpdate;
        Window.Load += onLoad;

        // Run the window.
        Window.Run();
        Dispose();
    }


    private void onLoad()
    {
        imguiController = new ImGuiController(
            Gl = Window.CreateOpenGL(), // load OpenGL
            Window, // pass in our window
            Input = Window.CreateInput() // create an input context
        );

        Logger = new Logger(LogOutput.Console);

        if (Input.Keyboards.Count < 1)
            throw new Exception("No ways youre actually tryna play without a keyboard\n for any of this shitty code to work you *need* a keyboard.");

        for (int i = 0; i < Input.Keyboards.Count; i++)
        {
            Input.Keyboards[i].KeyDown += onKeyDown;
        }

        ContentManager = new ContentManager();
        LoadEssentialAssets();


        GameScreenManager = new GameScreenManager(Gl);
        GameScreenManager.ChangeGameScreen(initialGameScreen);

    }

    private void LoadEssentialAssets()
    {
        ContentManager.GenerateNamedShader("material_basic", "Assets/material_shader/basic.vert", "Assets/material_shader/basic.frag");
        ContentManager.GenerateNamedShader("basic", "Assets/basic_shader/basic.vert", "Assets/basic_shader/basic.frag");
        ContentManager.GenerateNamedShader("material_advanced", "Assets/material_shader/advanced.vert", "Assets/material_shader/advanced.frag");

        ContentManager.GenerateNamedTexture("gray", "Assets/gray.png");
        ContentManager.GenerateNamedTexture("white", "Assets/white.png");
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
        GameScreenManager.Dispose();
        Window.Dispose();
    }
}

