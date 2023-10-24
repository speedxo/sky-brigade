using Horizon.Content;
using Horizon.OpenGL;

namespace Horizon.Rendering
{
    public class CustomMaterial : Material
    {
        public CustomMaterial(Shader shader)
        {
            this.Technique = new Technique(shader);
        }

        public CustomMaterial(Technique technique)
        {
            this.Technique = technique;
        }

        public override void Use(ref RenderOptions options) { }
    }
}
