namespace SkyBrigade.Engine.Rendering;

public struct DebugRenderOptions
{
    public DefferedRenderLayer DefferedLayer { get; set; }

    public static DebugRenderOptions Default { get; } = new DebugRenderOptions
    {
        DefferedLayer = DefferedRenderLayer.None
    };
}