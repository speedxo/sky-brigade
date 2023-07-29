using Silk.NET.OpenGL;
using System.Numerics;
using Texture = SkyBrigade.Engine.OpenGL.Texture;

namespace SkyBrigade.Engine.Rendering
{
    public class EmptyMaterial : Material
    {
        // forward some properties
        public override Vector4 Color { get; set; }

        public override Texture? Texture { get; set; }

        public EmptyMaterial()
        {
            Shader = GameManager.Instance.ContentManager.GetShader("basic");
            Color = Vector4.One;
        }

        public override void Load(string path)
        {
        }

        public override void Save(string path)
        {
        }

        public override void Use(RenderOptions? renderOptions = null)
        {
            var options = renderOptions ?? RenderOptions.Default;

            Shader.Use();
            Shader.SetUniform("uColor", Color * options.Color);

            if (Texture == null)
                Shader.SetUniform("useTexture", 0);
            else
            {
                GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
                GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Texture.Handle);
                Shader.SetUniform("uTexture", 0);
                Shader.SetUniform("useTexture", 1);
            }
        }
    }
}