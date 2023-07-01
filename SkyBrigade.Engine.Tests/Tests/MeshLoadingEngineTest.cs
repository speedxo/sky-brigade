using System;
using System.Numerics;
using System.Xml.Linq;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.Rendering;
using static Karma.CoreOBJ.OBJMesh;
using Mesh = SkyBrigade.Engine.Rendering.Mesh;
using Vertex = SkyBrigade.Engine.Data.Vertex;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class MeshLoadingEngineTest : IEngineTest
    {
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Mesh Loading Test";

        private Vector3 scale = Vector3.One;
        private Vector3 rot = Vector3.Zero;

        private List<Mesh> meshes;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            meshes = new List<Mesh>() {
                Mesh.CreateRectangle(),
                Mesh.FromObj("Assets/teapot.obj"),
                new Mesh(() => {
                    SphereGenerator.GenerateSphere(1.0f, 15, 15, out Vertex[] verts, out uint[] indices);
                    return (verts, indices);
                })
            };

            int counter = 0;
            foreach (var mesh in meshes)
            {
                mesh.Position = new Vector3(counter * 5, 0, 0);
                counter++;
            }


            Loaded = true;
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            foreach (var m in meshes)
                m.Draw(renderOptions);            
        }

        public void Update(float dt)
        {
            foreach (var mesh in meshes)
            {
                if (mesh.Scale != scale)
                    mesh.Scale = scale;
                if (mesh.Rotation != rot)
                    mesh.Rotation = rot;
            }
        }

        public void Dispose()
        {
            Loaded = false;
            foreach (var m in meshes)
                m.Dispose();

                GC.SuppressFinalize(this);
        }

        public void RenderGui()
        {
            ImGui.DragFloat3("Scale", ref scale, 0.01f);
            ImGui.DragFloat3("Rotation", ref rot, 0.1f);
        }
    }
}

