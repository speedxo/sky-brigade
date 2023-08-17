using System;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering
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
        public override void Use(RenderOptions? options = null)
        {

        }
    }
}

