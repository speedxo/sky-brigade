using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Game.Player;

namespace SkyBrigade.Game;

internal class DemoGameScreen : GameScreen
{
    private Plane rect;

    CharacterController character;

    public override void Initialize(GL gl)
    {
        base.Initialize(gl);

        character = new CharacterController();
        AddEntity(character);

        gl.ClearColor(System.Drawing.Color.CornflowerBlue);

        rect = new()
        {
            Rotation = new System.Numerics.Vector3(90, 0, 0),
            Scale = new System.Numerics.Vector2(10)
        };
        rect.Material.Texture = GameManager.Instance.ContentManager.GetTexture("white");

        
        gl.Enable(EnableCap.DepthTest);
    }

    public override void Render(GL gl, float dt)
    {
        base.Render(gl, dt);

        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        gl.Viewport(0, 0, (uint)GameManager.Instance.Window.FramebufferSize.X, (uint)GameManager.Instance.Window.FramebufferSize.Y);

        rect.Draw(RenderOptions.Default with
        {
            Camera = character.Camera
        });

        if (ImGui.Begin("Debug"))
        {
            ImGui.Text($"Memory Consumption: {float.Round(GC.GetTotalMemory(false) / 1024.0f / 1024, 2)}MB");
            ImGui.Text($"Memory Delta: {memoryTracker.GetAverage()}KB");
            ImGui.PlotLines("dKb/dt", ref memoryTracker.Buffer[0], memoryTracker.Buffer.Length);
            ImGui.Text($"FPS: {1.0f / dt}");
            ImGui.End();
        }

        if (ImGui.Begin("Character Controller"))
        {
            ImGui.Text($"Position: {character.Position}");
            ImGui.Text($"Rotation: {character.Rotation}");
            ImGui.End();
        }
    }

    private readonly DeltaTracker<float> memoryTracker = new((prev, current) => current - prev);

    public override void Update(float dt)
    {
        base.Update(dt);

        memoryTracker.Update(GC.GetTotalMemory(false) / 1024.0f);
    }


    public override void Dispose()
    {

    }
}