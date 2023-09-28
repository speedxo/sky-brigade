using Silk.NET.OpenGL;
using System.Numerics;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon.Rendering
{
    public class EmptyMaterial : Material
    {
        // forward some properties
        public override Vector4 Color { get; set; }

        public override Texture? Texture { get; set; }

        public EmptyMaterial()
        {
            Technique = AddEntity(new Technique("Assets/basic_shader", "basic"));
            Color = Vector4.One;
        }

        public override void Use(RenderOptions? renderOptions = null)
        {
            var options = renderOptions ?? RenderOptions.Default;

            Technique.Use();
            Technique.SetUniform("uColor", Color * options.Color);

            if (Texture == null)
                Technique.SetUniform("useTexture", 0);
            else
            {
                Engine.GL.ActiveTexture(TextureUnit.Texture0);
                Engine.GL.BindTexture(TextureTarget.Texture2D, Texture.Handle);
                Technique.SetUniform("uTexture", 0);
                Technique.SetUniform("useTexture", 1);
            }
        }
    }
}
