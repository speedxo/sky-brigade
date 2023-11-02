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
        public BasicParticle2DMaterial()
            : base(new Technique("assets/particle2d/", "basic")) { }
    }
}
