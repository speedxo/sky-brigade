using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Tests.Tests
{
    public interface IEngineTest : IDisposable
    {
        public bool Loaded { get; set; }
        public string Name { get; protected set; }

        void Render(float dt, GL gl, RenderOptions? renderOptions = null);

        void Update(float dt);

        void RenderGui();

        void LoadContent(GL gl);
    }
}