using Horizon.Rendering;
using Horizon.Rendering.Shapes;
using ImGuiNET;
using Vector3 = System.Numerics.Vector3;

namespace Horizon.Tests.Tests
{
    public class PlaneEngineTest : IEngineTest
    {
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Render Rectangle Test";

        private Plane rect;

        private Vector3 scale = Vector3.One;
        private Vector3 rot = Vector3.Zero;

        public PlaneEngineTest()
        {
            if (Loaded)
                return;

            rect = new Plane();

            Loaded = true;
        }

        public void Render(float dt, RenderOptions? renderOptions = null)
        {
            rect.Draw(dt, renderOptions);
        }

        public void Update(float dt)
        {
            if (rect.Scale != scale)
                rect.Scale = scale;
            if (rect.Rotation != rot)
                rect.Rotation = rot;
        }

        public void Dispose()
        {
            Loaded = false;

            GC.SuppressFinalize(this);
        }

        public void RenderGui()
        {
            ImGui.DragFloat3("Scale", ref scale, 0.01f);
            ImGui.DragFloat3("Rotation", ref rot, 0.1f);
        }
    }
}
