using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Tests.Tests;

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
            new RenderRectangleEngineTest(),
            new MeshLoadingEngineTest(),
            new PingPongGameTest(),
            new PIDTest()
        };

        for (int i = 0; i < tests.Count; i++)
            tests[i].LoadContent(gl);

        gl.ClearColor(System.Drawing.Color.Black);
        gl.Enable(EnableCap.DepthTest);
        gl.DepthFunc(DepthFunction.Lequal);
    }

    public void Render(GL gl, float dt)
    {
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (ImGui.Begin("information"))
        {
            ImGui.Text($"Test: '{tests[index].Name}' ({index + 1}/{tests.Count})");

            if (ImGui.ArrowButton("Prev", ImGuiDir.Left))
                index--;
            ImGui.SameLine();
            if (ImGui.ArrowButton("Next", ImGuiDir.Right))
                index++;

            index = Math.Clamp(index, 0, tests.Count - 1);

            tests[index].RenderGui();

            ImGui.End();
        }

        tests[index].Render(dt, gl, RenderOptions.Default with
        {
            Camera = testCamera
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