using System.Runtime.InteropServices;
using Horizon;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using ImGuiNET;

namespace Gravitator2D;

public class SimulatorScene : Horizon.Scene
{
    private Universe universe;

    public SimulatorScene()
    {
        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        universe = AddEntity<Universe>();

        InitializeRenderingPipeline();
    }


    public override void DrawGui(float dt)
    {
        if (ImGui.Begin("Physics"))
        {
            ImGui.DragFloat("time scale", ref universe.TimeScale, 0.01f);
            ImGui.DragFloat("G constant", ref universe.GravityConstant, 0.0001f);
            ImGui.DragFloat("Center Attraction", ref universe.CentralAttractionStrength, 0.001f);
            ImGui.DragFloat("Max speed", ref universe.MaxSpeed, 0.1f);
            ImGui.DragFloat("Coefficient of Restitution", ref universe.CoRestitution, 0.01f, 0.01f, 1.0f);
            ImGui.DragFloat("Close Repulsion", ref universe.RepulsionStrength, 0.01f, 0.01f, 10.0f);
            ImGui.End();
        }
    }

    public override void DrawOther(float dt, RenderOptions? renderOptions = null)
    {
        
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
