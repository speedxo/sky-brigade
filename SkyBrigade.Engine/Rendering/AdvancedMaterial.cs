using Silk.NET.OpenGL;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering
{
    public class AdvancedMaterial : Material
    {
        public AdvancedMaterialDescription MaterialDescription { get; set; }

        public AdvancedMaterial()
        {
            Shader = GameManager.Instance.ContentManager.GetShader("material_advanced");
            MaterialDescription = AdvancedMaterialDescription.Default;
        }

        public override void Use()
        {
            Shader.Use();

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.Metallicness.Handle);
            Shader.SetUniform("uMetallicness", 0);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture1);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.Roughness.Handle);
            Shader.SetUniform("uRoughness", 1);

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture2);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, MaterialDescription.AmbientOcclusion.Handle);
            Shader.SetUniform("uAo", 2);
        }
    }
}

