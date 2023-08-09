using System;
using System.Collections.Generic;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering.Effects.Components;

namespace SkyBrigade.Engine.Rendering.Effects
{
	public class Effect : Entity
	{
		public ShaderComponent Shader { get; init; }

		public Effect(string path, string fileName)
		{
			Shader = AddComponent(new ShaderComponent(
				Path.Combine(path, fileName, ".vert"),
                Path.Combine(path, fileName, ".frag")));
		}

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            base.Draw(dt, renderOptions);

        }
        public override void Update(float dt)
        {
            base.Update(dt);

		}
    }

	public class EffectStack
	{
		public LinkedList<Effect> Effects { get; private init; }

		public EffectStack()
		{
			Effects = new LinkedList<Effect>();
		}
	}
}

