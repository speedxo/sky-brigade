using System;
using Horizon.OpenGL;

namespace Horizon.Rendering.Effects
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

