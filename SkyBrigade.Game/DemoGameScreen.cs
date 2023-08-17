using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Rendering.Shapes;
using SkyBrigade.Engine.Prefabs.Character;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering.Effects;
using SkyBrigade.Engine.Rendering.Effects.Components;

namespace SkyBrigade.Game;

internal class DemoGameScreen : Scene
{
    private Plane plane;
    private CharacterController character;

    public DemoGameScreen()
        :base()
    {
        InitializeRenderingPipeline();

        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.DepthTest);

        character = new CharacterController();
        AddEntity(character);

        plane = new Plane(new BasicMaterial());
        
        plane.Transform.Scale = new System.Numerics.Vector3(10);
        plane.Transform.Rotation = new System.Numerics.Vector3(90, 0, 0);

        plane.Material.Texture = GameManager.Instance.ContentManager.GetTexture("debug");

        AddEntity(plane);

        GameManager.Instance.Debugger.Enabled = true;
    }

    protected override Effect[] GeneratePostProccessingEffects()
    {
        //return Array.Empty<Effect>();
        return new[] {  new FlashingEffect() }; 
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        var options = (renderOptions ?? RenderOptions.Default) with
        {
            Camera = character.Camera
        };

        base.Draw(dt, options);
    }

    public override void DrawGui(float dt)
    {
        if (ImGui.Begin("Character Controller"))
        {
            ImGui.Text($"Position: {character.Position}");
            ImGui.Text($"Rotation: {character.Rotation}");
            ImGui.End();
        }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (GameManager.Instance.InputManager.WasPressed(Engine.Input.VirtualAction.Interact))
            GameManager.Instance.Debugger.Enabled = !GameManager.Instance.Debugger.Enabled;
    }


    public override void Dispose()
    {

    }
}