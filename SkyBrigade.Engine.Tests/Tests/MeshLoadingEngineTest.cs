using System;
using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class MeshLoadingEngineTest : IEngineTest
    {
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Mesh Loading Test";

        private Mesh mesh;
        private Vector3 scale = Vector3.One;
        private Vector3 rot = Vector3.Zero;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            //mesh = Mesh.CreateRectangle();
            mesh = Mesh.FromObj("Assets/teapot.obj");

            Loaded = true;
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            mesh.Draw(renderOptions);            
        }

        public void Update(float dt)
        {
            if (mesh.Scale != scale)
                mesh.Scale = scale;
            if (mesh.Rotation != rot)
                mesh.Rotation = rot;
        }

        public void Dispose()
        {
            Loaded = false;
            mesh.Dispose();

            GC.SuppressFinalize(this);
        }

        public void RenderGui()
        {
            ImGui.DragFloat3("Scale", ref scale, 0.01f);
            ImGui.DragFloat3("Rotation", ref rot, 0.1f);
        }
    }
}

