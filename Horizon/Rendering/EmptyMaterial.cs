using Silk.NET.OpenGL;
using System.Numerics;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon.Rendering
{
    public class EmptyMaterial : Material
    {
        private const string UNIFORM_COLOR = "uColor";
        private const string UNIFORM_TEXTURE = "uTexture";
        private const string UNIFORM_USE_TEXTURE = "useTexture";

        // forward some properties
        public override Vector4 Color { get; set; }

        public override Texture? Texture { get; set; }

        public EmptyMaterial()
        {
            Technique = AddEntity(new Technique("Assets/basic_shader", "basic"));
            Color = Vector4.One;
        }

        public override void Use(in RenderOptions options)
        {
            Technique.Use();
            Technique.SetUniform(UNIFORM_COLOR, Color * options.Color);

            if (Texture == null)
                Technique.SetUniform(UNIFORM_USE_TEXTURE, 0);
            else
            {
                Engine.GL.ActiveTexture(TextureUnit.Texture0);
                Engine.GL.BindTexture(TextureTarget.Texture2D, Texture.Handle);
                Technique.SetUniform(UNIFORM_TEXTURE, 0);
                Technique.SetUniform(UNIFORM_USE_TEXTURE, 1);
            }
        }
    }
}
