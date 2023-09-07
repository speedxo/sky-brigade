using Horizon;
using Horizon.Debugging.Debuggers;
using Horizon.Prefabs.Character;
using Horizon.Prefabs.Effects;
using Horizon.Rendering;
using Horizon.Rendering.Effects;
using Horizon.Rendering.Shapes;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace SkyBrigade;

internal class DemoGameScreen : Scene
{
    private Plane plane;
    private CharacterController character;
    private VingetteEffect vingette;

    public DemoGameScreen()
    {
        InitializeRenderingPipeline();

        /* need to consoldiate OpenGL calls into profiles, perhaps a call to
         * GameManager.Instance.InitializeGL(GLProfile); maybe predefined calls
         * ie. GLProfile.Realtime3D can setup depth testing and GLProfile.Flat
         * can be 2D orientated?                                              */
        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.DepthTest);

        character = new CharacterController();
        AddEntity(character);

        /* We cannot use property initialisation here due to the fact that each
         * component is instantiated using reflection and it monitored by the
         * entity class.                                                     */

        plane = new Plane(new BasicMaterial());
        plane.Transform.Scale = new System.Numerics.Vector3(10);
        plane.Transform.Rotation = new System.Numerics.Vector3(90, 0, 0);
        plane.Material.Texture = GameManager.Instance.ContentManager.GetTexture("debug");
        AddEntity(plane);

        // yea this single boolean enables or disables the debugging interface
        GameManager.Instance.Debugger.Enabled = true;

        GameManager.Instance.Debugger.GeneralDebugger.AddWatch(
            "player pos",
            () => character.Position
        );
    }

    /* The reason i opted to have an overidable method is for safety, while its
     * not in the slightest ellegant, what it is is foolproof and simple.     */

    protected override Effect[] GeneratePostProccessingEffects()
    {
        return new Effect[] { vingette = new VingetteEffect() };
    }

    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        var options = (renderOptions ?? RenderOptions.Default) with { Camera = character.Camera };

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
        if (ImGui.Begin("Effects"))
        {
            foreach (var effect in PostEffects.Effects)
            {
                SceneEntityDebugger.DrawProperties(vingette);
                ImGui.Text($"{effect.BindingPoint}");
            }

            ImGui.End();
        }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (GameManager.Instance.InputManager.WasPressed(Horizon.Input.VirtualAction.Interact))
            GameManager.Instance.Debugger.Enabled = !GameManager.Instance.Debugger.Enabled;
    }

    public override void Dispose() { }

    public override void DrawOther(float dt, RenderOptions? renderOptions = null) { }
}
