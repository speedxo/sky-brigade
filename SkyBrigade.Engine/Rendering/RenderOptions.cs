using System.Numerics;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Rendering;

// This is about to be bad

public struct RenderOptions
{
	public Vector4 Color { get; set; }
	public Camera Camera { get; set; }
	public Texture Texture { get; set; }

	public static RenderOptions Default { get; } = new RenderOptions
	{
		Color = Vector4.One,
		Camera = new Camera() { Position = new Vector3(0, 0, 10) },
		Texture = GameManager.Instance.ContentManager.GetTexture("white")
	};
}

