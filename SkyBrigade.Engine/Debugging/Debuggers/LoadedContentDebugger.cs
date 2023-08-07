using System;
using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging.Debuggers
{
	public class LoadedContentDebugger : IGameComponent
	{
        public string Name { get; set; }

        public Entity Parent { get; set; }
        private Debugger Debugger { get; set; }


        public void Initialize()
        {
            Debugger = Parent as Debugger;
        }

        public bool Visible = false;

        public void Draw(float dt, RenderOptions? options = null)
        {
            if (!Visible) return;

        }

        public void Update(float dt)
        {
            
        }
    }
}

