using System.Numerics;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Rendering;

// This is about to be bad

public struct RenderOptions
{
	public Vector4 Color { get; set; }
	public Shader Shader { get; set; }
	public Camera Camera { get; set; }
	public Texture Texture { get; set; }

	public static RenderOptions Default { get; } = new RenderOptions
	{
		Color = Vector4.One,
		Shader = GameManager.Instance.ContentManager.GenerateNamedShader("basic", new Shader(GameManager.Instance.Gl, "Assets/basic_shader/basic.vert", "Assets/basic_shader/basic.frag")),
		Camera = new Camera() { Position = Vector3.Zero },
		Texture = GameManager.Instance.ContentManager.GenerateNamedTexture("debug", "Assets/among.png")
	};
}

