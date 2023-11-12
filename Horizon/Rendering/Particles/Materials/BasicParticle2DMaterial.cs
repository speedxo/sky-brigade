using Horizon.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Rendering.Particles.Materials
{
    public class BasicParticle2DMaterial : CustomMaterial
    {
        private const string UNIFORM_STARTCOLOR = "uStartColor";
        private const string UNIFORM_ENDCOLOR = "uEndColor";
        private ParticleRenderer2D renderer;
        private bool initialized = false;
        public BasicParticle2DMaterial()
            : base(new Technique("assets/particle2d/", "basic")) { }

        public override void Initialize()
        {
            base.Initialize();

            if (Parent is null)
                throw new Exception("Parent is null, did you forget to call AddEntity()?");

            if (Parent is not ParticleRenderer2D) // pattern matching :pig_head_explosion:
            {
                string error = "Attempt to attach Particle Material to an entity which is not a particle renderer!";
                Engine.Logger.Log(Logging.LogLevel.Error, error);
                throw new Exception(error);
            }

            renderer = (ParticleRenderer2D)Parent!;
            initialized = true; // :)
        }

        public override void Use(in RenderOptions options)
        {
            base.Use(options);

            if (!initialized) return;
            Technique.SetUniform(UNIFORM_STARTCOLOR, renderer.StartColor);
            Technique.SetUniform(UNIFORM_ENDCOLOR, renderer.EndColor);
        }
    }
}
