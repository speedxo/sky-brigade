using Silk.NET.OpenGL;
using System.Numerics;

namespace Horizon.Rendering;

// This is about to be bad

public struct RenderOptions
{
    // Primative Properties
    public float Gamma { get; set; }

    public bool IsPostProcessingEnabled { get; set; }
    public bool IsWireframeEnabled { get; set; }
    public float AmbientLightingStrength { get; set; }
    public bool IsBox2DDebugDrawEnabled { get; set; }

    // Reference Properties
    public Camera Camera { get; set; }

    public GL GL { get; set; }

    // Value Properties
    public Vector4 Color { get; set; }

    public DebugRenderOptions DebugOptions { get; set; }

    public static RenderOptions Default = new RenderOptions
    {
        Camera = new Camera() { Position = new Vector3(0, 0, 10) },
        Color = Vector4.One,
        IsWireframeEnabled = false,
        Gamma = 2.2f,
        AmbientLightingStrength = 0.03f,
        DebugOptions = DebugRenderOptions.Default,
        IsPostProcessingEnabled = true,
        GL = GameEntity.Entity.Engine.GL,
        IsBox2DDebugDrawEnabled = false
    };
}
