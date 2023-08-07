using System.Numerics;
using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.Rendering;

// This is about to be bad

public struct RenderOptions
{
    // Primative Properties
    public float Gamma { get; set; }

    public float AmbientLightingStrength { get; set; }

    // Reference Properties
    public Camera Camera { get; set; }
    public GL GL { get; set; }

    // Value Properties
    public Vector4 Color { get; set; }

    public DebugRenderOptions DebugOptions { get; set; }

    public static RenderOptions Default { get; } = new RenderOptions
    {
        Camera = new Camera() { Position = new Vector3(0, 0, 10) },
        Color = Vector4.One,
        Gamma = 2.2f,
        AmbientLightingStrength = 0.03f,
        DebugOptions = DebugRenderOptions.Default,
        GL = GameManager.Instance.Gl
    };
}