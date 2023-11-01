using Box2D.NetStandard.Common;
using Horizon.Content;
using Horizon.OpenGL;

namespace Horizon.Rendering
{
    public class CustomMaterial : Material
    {
        public CustomMaterial(in Shader shader)
        {
            this.Technique = new Technique(shader);
        }
        public CustomMaterial(in Technique technique)
        {
            this.Technique = technique;
        }


        public CustomMaterial(in Texture texture, in Shader shader)
        {
            this.Texture = texture;
            this.Technique = new Technique(shader);
        }

        public CustomMaterial(in Texture texture, in Technique technique)
        {
            this.Texture = texture;
            this.Technique = technique;
        }

        public override void Use(in RenderOptions options) 
        {
            Technique.Use();
            Technique.SetUniform("uView", options.Camera.View);
            Technique.SetUniform("uProjection", options.Camera.Projection);
            Technique.SetUniform("uWireframeEnabled", options.IsWireframeEnabled ? 1 : 0);
            Texture?.Bind(Silk.NET.OpenGL.TextureUnit.Texture0);
        }
    }
}
