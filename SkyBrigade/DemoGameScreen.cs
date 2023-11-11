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
         * Engine.InitializeGL(GLProfile); maybe predefined calls
         * ie. GLProfile.Realtime3D can setup depth testing and GLProfile.Flat
         * can be 2D orientated?                                              */
        Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
        Engine.GL.Enable(EnableCap.DepthTest);

        character = new CharacterController(
            CharacterController.CharacterMovementControllerConfig.Default
        );
        AddEntity(character);

        /* We cannot use property initialisation here due to the fact that each
         * component is instantiated using reflection and it monitored by the
         * entity class.                                                     */

        plane = new Plane(new BasicMaterial());
        plane.Transform.Scale = new System.Numerics.Vector3(10);
        plane.Transform.Rotation = new System.Numerics.Vector3(90, 0, 0);
        plane.Material.Texture = Engine.Content.GetTexture("white");

        AddEntity(plane);

        Engine.Debugger.GeneralDebugger.AddWatch("player pos", () => character.Position);
    }

    /* The reason i opted to have an overidable method is for safety, while its
     * not in the slightest ellegant, what it is is foolproof and simple.     */

    protected override Effect[] GeneratePostProccessingEffects()
    {
        return new Effect[] { vingette = new VingetteEffect() };
    }

    public override void Render(float dt, ref RenderOptions options)
    {
        var opts = options with { Camera = character.Camera };

        base.Render(dt, ref opts);
    }

    public override void DrawGui(float dt)
    {
        if (ImGui.Begin("Character Controller"))
        {
            ImGui.Text($"Position: {character.Position}");
            ImGui.Text($"Rotation: {character.Rotation}");
            ImGui.End();
        }

        var mat = (BasicMaterial)plane.Material;

        if (ImGui.Begin("Material"))
        {
            ImGui.DragFloat("AO", ref mat.MaterialDescription.AmbientOcclusion);
            ImGui.ColorEdit4("Color", ref mat.MaterialDescription.Color);

            ImGui.End();
        }
    }

    public override void UpdateState(float dt)
    {
        base.UpdateState(dt);

        //if (Engine.Input.WasPressed(Horizon.Input.VirtualAction.Interact))
        //    Engine.Debugger.Enabled = !Engine.Debugger.Enabled;
    }

    public override void Dispose() { }

    public override void DrawOther(float dt, ref RenderOptions options) { }
}
