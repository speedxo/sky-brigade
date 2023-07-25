using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Collections;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;

using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Game;

internal class DemoGameScreen : IGameScreen
{
    private Plane rect;
    private Camera testCamera;
    private Texture testTexture;
    private Mesh testMesh;

    public void Initialize(GL gl)
    {
        gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

        rect = new();

        testCamera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5) };

        gl.Enable(EnableCap.DepthTest);
    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);

        rect.Draw(RenderOptions.Default with
        {
            Camera = testCamera,
            Color = new System.Numerics.Vector4(MathF.Sin(timer * 0.5f), MathF.Sin(timer * 1.4f), MathF.Sin(timer), 1.0f),
            Texture = GameManager.Instance.ContentManager.GetTexture("debug")
        });

        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Memory Consumption: {float.Round(GC.GetTotalMemory(false) / 1024.0f / 1024, 2)}MB");
            ImGui.Text($"Memory Delta: {memoryTracker.GetAverage()}KB");
            ImGui.PlotLines("dKb/dt", ref memoryTracker.Buffer[0], memoryTracker.Buffer.Length);
            ImGui.End();
        }
    }
    private DeltaTracker<float> memoryTracker = new DeltaTracker<float>((prev, current) => current - prev);
    private float timer = 0.0f, memoryTimer = 0.0f;

    public void Update(float dt)
    {
        memoryTracker.Update(GC.GetTotalMemory(false) / 1024.0f);

        timer += dt * 10.0f;
        memoryTimer += dt;
        if (memoryTimer > 1.0f)
        {
            GC.Collect();
            memoryTimer = 0.0f;
        }


        testCamera.Update(dt);
        rect.Rotation += dt * 100.0f;
    }

    public void Dispose()
    {
    }
}