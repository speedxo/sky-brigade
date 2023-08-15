using System;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering.Effects
{
	public class BasicEffect : Effect
	{
        public BasicEffect() :
            base(File.ReadAllText("Assets/effects/basicStage.frag"))
        {
            
        }

        public override void UpdateBuffer(float dt, in UniformBufferObject bufferObject)
        {

        }
    }
}

