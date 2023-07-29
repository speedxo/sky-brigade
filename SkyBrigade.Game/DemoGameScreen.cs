using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Game;

internal class DemoGameScreen : IGameScreen
{
    private Plane rect;
    private Camera testCamera;

    public void Initialize(GL gl)
    {
        gl.ClearColor(System.Drawing.Color.CornflowerBlue);

        rect = new()
        {
            Rotation = new System.Numerics.Vector3(90, 0, 0),
            Scale = new System.Numerics.Vector2(10)
        };
        rect.Material.Texture = GameManager.Instance.ContentManager.GetTexture("white");

        testCamera = new Camera() { Position = new System.Numerics.Vector3(0, 2, 5) };

        gl.Enable(EnableCap.DepthTest);
    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);

        rect.Draw(RenderOptions.Default with
        {
            Camera = testCamera
        });


        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Memory Consumption: {float.Round(GC.GetTotalMemory(false) / 1024.0f / 1024, 2)}MB");
            ImGui.Text($"Memory Delta: {memoryTracker.GetAverage()}KB");
            ImGui.PlotLines("dKb/dt", ref memoryTracker.Buffer[0], memoryTracker.Buffer.Length);
            ImGui.Text($"FPS: {1.0f / dt}");
            ImGui.End();
        }
    }

    private readonly DeltaTracker<float> memoryTracker = new((prev, current) => current - prev);

    public void Update(float dt)
    {
        memoryTracker.Update(GC.GetTotalMemory(false) / 1024.0f);

        testCamera.Update(dt);
    }

    public void Dispose()
    {
    }
}