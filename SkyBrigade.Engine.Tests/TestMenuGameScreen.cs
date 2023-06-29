using System;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Tests.Tests;

namespace SkyBrigade.Engine.Tests;

public class TestMenuGameScreen : IGameScreen
{
    private int index = 0;
    private List<IEngineTest> tests;
    private Camera testCamera;

    public void Initialize(GL gl)
    {
        testCamera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5) };

        tests = new List<IEngineTest>() {
            new RenderRectangleEngineTest(),
            new MeshLoadingEngineTest()
        };

        for (int i = 0; i < tests.Count; i++)
            tests[i].LoadContent(gl);

        gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        gl.Enable(EnableCap.DepthTest);

    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);

        if (ImGui.Begin("information"))
        {
            ImGui.Text($"Test: '{tests[index].Name}' ({index + 1}/{tests.Count})");
            tests[index].RenderGui();
            ImGui.End();
        }

        tests[index].Render(dt, gl, RenderOptions.Default with {
            Camera = testCamera
        });
    }

    public void Update(float dt)
    {
        if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Silk.NET.Input.Key.Right))
            index++;
        else if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Silk.NET.Input.Key.Left))
            index--;
        index = Math.Clamp(index, 0, tests.Count - 1);

        testCamera.Update(dt);  
        tests[index].Update(dt);
    }
    public void Dispose()
    {
        for (int i = 0; i < tests.Count; i++)
            tests[i].Dispose();
    }
}

