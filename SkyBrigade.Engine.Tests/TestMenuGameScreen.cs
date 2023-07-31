﻿using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Logging;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Tests.Tests;

using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Tests;

public class TestMenuGameScreen : IGameScreen
{
    private int index = 0;
    private IKeyboard prev;
    private Camera testCamera;
    private List<IEngineTest> tests;

    public void Dispose()
    {
        for (int i = 0; i < tests.Count; i++)
            tests[i].Dispose();
    }

    public void Initialize(GL gl)
    {
        testCamera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5) };

        tests = new List<IEngineTest>() {
            new PlaneEngineTest(),
            new MeshLoadingEngineTest(),
            new PlanetGraphicsTest(),
            new PingPongGameTest(),
            new PIDTest()
        };

        for (int i = 0; i < tests.Count; i++)
            tests[i].LoadContent(gl);

        gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Lequal);

        gamma = RenderOptions.Default.Gamma;
        ambientStrength = RenderOptions.Default.AmbientLightingStrength;

        // automatically generate the render modes from the DefferedRenderLayer enum
        renderModes = Enum.GetNames(typeof(DefferedRenderLayer));

        GameManager.Instance.Logger.Log(LogLevel.Info, $"Texture constructor call count: {Texture.count}");
    }

    private DeltaTracker<float> memoryTracker = new DeltaTracker<float>((prev, current) => current - prev);
    private bool showDebugWindow = true;
    private float gamma, ambientStrength;
    private string[] renderModes;
    private int renderModeIndex;

    public void Render(GL gl, float dt)
    {
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                // Menu item for showing/hiding the Debug Information window.
                if (ImGui.MenuItem("Close"))
                    GameManager.Instance.Window.Close();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Debug"))
            {
                // Menu item for showing/hiding the Debug Information window.
                ImGui.MenuItem("Show Debug Panel", "", ref showDebugWindow);

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        if (showDebugWindow)
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X - 350, 0), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(350, ImGui.GetIO().DisplaySize.Y), ImGuiCond.Always);

            if (ImGui.Begin("Sidebar", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings |
                                    ImGuiWindowFlags.MenuBar))
            {
                // Collapsible header for Debug Info window
                if (ImGui.CollapsingHeader("Debug Info", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Text($"Test: '{tests[index].Name}' ({index + 1}/{tests.Count})");

                    if (ImGui.ArrowButton("Prev", ImGuiDir.Left))
                        index--;
                    ImGui.SameLine();
                    if (ImGui.ArrowButton("Next", ImGuiDir.Right))
                        index++;

                    index = Math.Clamp(index, 0, tests.Count - 1);

                    tests[index].RenderGui();
                }

                // Collapsible header for Memory Usage window
                if (ImGui.CollapsingHeader("Memory Usage", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Text($"Memory Consumption: {float.Round((GC.GetTotalMemory(false) / 1024.0f) / 1024, 2)}MB");
                    ImGui.Text($"Memory Delta: {memoryTracker.GetAverage()}KB");
                    ImGui.PlotLines("dKb/dt", ref memoryTracker.Buffer[0], memoryTracker.Buffer.Length);
                }

                // Collapsible header for Render Options window
                if (ImGui.CollapsingHeader("Render Options", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.DragFloat("Gamma", ref gamma, 0.01f, 0.1f, 10.0f);
                    ImGui.DragFloat("Ambient", ref ambientStrength, 0.01f, 0.0f, 10.0f);

                    // Listbox to select the render mode.
                    ImGui.Combo("Render Mode", ref renderModeIndex, renderModes, renderModes.Length);
                }

                // End the sidebar layout
                ImGui.End();
            }
        }


        tests[index].Render(dt, gl, RenderOptions.Default with
        {
            Camera = testCamera,
            AmbientLightingStrength = ambientStrength,
            Gamma = gamma,
            DebugOptions = DebugRenderOptions.Default with
            {
                DefferedLayer = (DefferedRenderLayer)renderModeIndex
            }
        });
    }

    public void Update(float dt)
    {
        var current = GameManager.Instance.Input.Keyboards[0];

        //if (current.IsKeyPressed(Key.Right) && prev.IsKeyPressed(Key.Right))
        //    index++;
        //if (current.IsKeyPressed(Key.Left) && prev.IsKeyPressed(Key.Left))
        //    index--;

        testCamera.Update(dt);
        tests[index].Update(dt);

        prev = current;
    }
}