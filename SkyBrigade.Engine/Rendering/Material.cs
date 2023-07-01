using Microsoft.Extensions.Options;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.OpenGL;
using Shader = SkyBrigade.Engine.OpenGL.Shader;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Rendering
{
	public struct BasicMaterialDescription
	{
		public float Metallicness { get; set; }
		public float Roughness { get; set; }
		public float AmbientOcclusion { get; set; }
		public static BasicMaterialDescription Default { get; } = new BasicMaterialDescription {
			Roughness = 0.25f,
			AmbientOcclusion = 1.0f,
			Metallicness  = 0.25f
		};
	}

	public struct AdvancedMaterialDescription
	{
        public Texture Metallicness { get; set; }
        public Texture Roughness { get; set; }
        public Texture AmbientOcclusion { get; set; }
        public static AdvancedMaterialDescription Default { get; } = new AdvancedMaterialDescription
        {
            Roughness = GameManager.Instance.ContentManager.GetTexture("gray"),
            AmbientOcclusion = GameManager.Instance.ContentManager.GetTexture("white"),
            Metallicness = GameManager.Instance.ContentManager.GetTexture("gray")
        };
    }   

    public abstract class Material
    {
        protected static Shader shader;
        public abstract void SetMaterial<T>(T material) where T: struct;
        public abstract void Use();
        public void End() => GameManager.Instance.Gl.UseProgram(0);
    }

    public class BasicMaterial : Material
    {
        static BasicMaterial()
        {
            shader = GameManager.Instance.ContentManager.GetShader("material_basic");
        }

        private BasicMaterialDescription desc;

        public BasicMaterial()
        {
            desc = BasicMaterialDescription.Default;
        }

        public override void SetMaterial<T>(T material) where T: struct
        {
            if (material is not BasicMaterialDescription) return;

            desc = (BasicMaterialDescription)material;
        }

        public override void Use()
        {
            shader.Use();
            shader.SetUniform("uMetallicness", desc.Metallicness);
            shader.SetUniform("uRoughness", desc.Roughness);
            shader.SetUniform("uAo", desc.AmbientOcclusion);
            shader.End();
        }
    }
    public class AdvancedMaterial : Material
    {
        static AdvancedMaterial()
        {
            shader = GameManager.Instance.ContentManager.GetShader("material_advanced");
        }

        private AdvancedMaterialDescription desc;

        public AdvancedMaterial()
        {
            desc = AdvancedMaterialDescription.Default;
        }

        public override void SetMaterial<AdvancedMaterialDescription>(AdvancedMaterialDescription material)
        {
            desc = material;
        }

        public override void Use()
        {
            shader.Use();

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, desc.Metallicness.Handle);
            shader.SetUniform("uMetallicness", 0);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture1);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, desc.Roughness.Handle);
            shader.SetUniform("uRoughness", 1);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture2);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, desc.AmbientOcclusion.Handle);
            shader.SetUniform("uAo", 2);

            shader.End();
        }
    }
}

