﻿using Horizon.Data;
using Horizon.Logging;
using Horizon.Rendering;
using Horizon.Tests.Tests;
using ImGuiNET;
using Silk.NET.OpenGL;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon.Tests;

public class TestMenuGameScreen : Scene
{
    private int index = 0;
    private Horizon.Prefabs.Character.CharacterController character;
    private List<IEngineTest> tests;

    public TestMenuGameScreen()
    {
        character = AddEntity(
            new Prefabs.Character.CharacterController()
            {
                Position = new System.Numerics.Vector3(0, 0, 5)
            }
        );

        tests = new List<IEngineTest>()
        {
            new PlaneEngineTest(),
            new MeshLoadingEngineTest(),
            new PlanetGraphicsTest(),
            new PingPongGameTest(),
            new PIDTest()
        };

        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.DepthTest);
        GameManager.Instance.Gl.DepthFunc(DepthFunction.Lequal);

        GameManager.Instance.Debugger.Enabled = true;

        GameManager.Instance.Logger.Log(
            LogLevel.Info,
            $"Texture constructor call count: {Texture.count}"
        );

        InitializeRenderingPipeline();
    }

    private DeltaTracker<float> memoryTracker = new DeltaTracker<float>(
        (prev, current) => current - prev
    );
    private bool showDebugWindow = true;

    public override void DrawGui(float dt)
    {
        if (ImGui.BeginMainMenuBar())
        {
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
            ImGui.SetNextWindowPos(
                new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X - 350, 0),
                ImGuiCond.Always
            );
            ImGui.SetNextWindowSize(
                new System.Numerics.Vector2(350, ImGui.GetIO().DisplaySize.Y),
                ImGuiCond.Always
            );

            if (
                ImGui.Begin(
                    "Sidebar",
                    ImGuiWindowFlags.NoTitleBar
                        | ImGuiWindowFlags.NoResize
                        | ImGuiWindowFlags.NoMove
                        | ImGuiWindowFlags.NoCollapse
                        | ImGuiWindowFlags.NoSavedSettings
                        | ImGuiWindowFlags.MenuBar
                )
            )
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
                    ImGui.Text(
                        $"Memory Consumption: {float.Round((GC.GetTotalMemory(false) / 1024.0f) / 1024, 2)}MB"
                    );
                    ImGui.Text($"Memory Delta: {memoryTracker.GetAverage()}KB");
                    ImGui.PlotLines(
                        "dKb/dt",
                        ref memoryTracker.Buffer[0],
                        memoryTracker.Buffer.Length
                    );
                }

                // End the sidebar layout
                ImGui.End();
            }
        }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        tests[index].Update(dt);
    }

    public override void Dispose()
    {
        for (int i = 0; i < tests.Count; i++)
            tests[i].Dispose();
    }

    public override void DrawOther(float dt, RenderOptions? renderOptions = null)
    {
        var options = (renderOptions ?? RenderOptions.Default) with { Camera = character.Camera };

        tests[index].Render(dt, options);
    }
}
