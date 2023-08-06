using System;
using SkyBrigade.Engine.Debugging.Debuggers;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging
{
	public class Debugger : Entity
	{
		public bool IsVisible { get; set; }

		public RenderOptionsDebugger RenderOptionsDebugger { get; init; }

		public Debugger()
		{
			RenderOptionsDebugger = AddComponent<RenderOptionsDebugger>();
		}

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (!IsVisible) return;

            base.Draw(dt, renderOptions);
        }
    }
}

