using ImGuiNET;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class RenderRectangleEngineTest : IEngineTest
    {
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Render Rectangle Test";

        private RenderRectangle rect;

        private Vector2 scale = Vector2.One;
        private float rot = 0.0f;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            rect = new RenderRectangle();

            Loaded = true;
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            rect.Draw(renderOptions);
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
            ImGui.DragFloat2("Scale", ref scale, 0.01f);
            ImGui.DragFloat("Rotation", ref rot, 0.1f);
        }
    }
}