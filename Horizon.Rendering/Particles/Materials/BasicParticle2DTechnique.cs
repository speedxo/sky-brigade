using Horizon.Content;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Rendering.Particles.Materials
{
    public class BasicParticle2DTechnique : Technique
    {
        private const string UNIFORM_STARTCOLOR = "uStartColor";
        private const string UNIFORM_ENDCOLOR = "uEndColor";
        private ParticleRenderer2D renderer;
        private bool initialized = false;

        public BasicParticle2DTechnique(in ParticleRenderer2D renderer, in Shader tech)
            : base(tech) { this.renderer = renderer; }


        protected override void SetUniforms()
        {
            SetUniform(UNIFORM_STARTCOLOR, renderer.StartColor);
            SetUniform(UNIFORM_ENDCOLOR, renderer.EndColor);
        }
    }
}
