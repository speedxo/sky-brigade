using System;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Tests.Tests
{
	public class RenderRectangleEngineTest : IEngineTest
	{
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Render Rectangle Test";

        RenderRectangle? rect;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            rect = new RenderRectangle();

            Loaded = true;
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions=null)
        {
            rect?.Draw(renderOptions);
        }

        public void Update(float dt)
        {
            
        }

        public void Dispose()
        {
            Loaded = false;
            rect = null;
                
            GC.SuppressFinalize(this);
        }
    }
}

