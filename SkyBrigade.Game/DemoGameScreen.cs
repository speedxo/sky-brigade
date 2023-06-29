using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Rendering;

using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Game;

class DemoGameScreen : IGameScreen
{
    private RenderRectangle rect;
    private Camera testCamera;
    private Texture testTexture;

    public void Initialize(GL gl)
    {
        gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

        rect = new();
        testTexture = GameManager.Instance.ContentManager.GenerateNamedTexture("amongus", "Assets/among.png");

        testCamera = new Camera() { Position = new System.Numerics.Vector3(0, 0, 5) };
    }

    public void Render(GL gl, float dt)
    {
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);

        rect.Draw(RenderOptions.Default with { Camera = testCamera, Texture = testTexture, Color = new System.Numerics.Vector4(MathF.Sin(timer * 0.5f), MathF.Sin(timer * 1.4f), MathF.Sin(timer), 1.0f) });

        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Memory Consumptions: {GC.GetTotalMemory(false) / 1000000.0}MB");
            ImGui.End();
        }
    }

    float timer = 0.0f;
    public void Update(float dt)
    {
        timer += dt * 10.0f;
        testCamera.Update(dt);

        rect.Rotation += dt * 100.0f;
    }

    public void Dispose()
    {

    }

}

