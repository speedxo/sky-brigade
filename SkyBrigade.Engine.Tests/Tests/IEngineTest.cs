using Silk.NET.OpenGL;
using Horizon.Rendering;

namespace Horizon.Tests.Tests
{
    public interface IEngineTest : IDisposable
    {
        public bool Loaded { get; set; }
        public string Name { get; protected set; }

        void Render(float dt, RenderOptions? renderOptions = null);

        void Update(float dt);

        void RenderGui();
    }
}