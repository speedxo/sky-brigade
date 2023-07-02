using System;
using System.Numerics;
using System.Xml.Linq;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
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

         // lights
    // ------
    Vector3[] lightPositions = new [] {
        new Vector3(-10.0f,  10.0f, 10.0f),
        new Vector3( 10.0f,  10.0f, 10.0f),
        new Vector3(-10.0f, -10.0f, 10.0f),
        new Vector3( 10.0f, -10.0f, 10.0f)
    };

        Vector3[] lightColors = new []{
        new Vector3(300.0f, 300.0f, 300.0f),
        new Vector3(300.0f, 300.0f, 300.0f),
        new Vector3(300.0f, 300.0f, 300.0f),
        new Vector3(300.0f, 300.0f, 300.0f) 
    };

        private BasicMaterial mat;
        private Mesh lightMesh;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            mat = new BasicMaterial();

            meshes = new List<Mesh>() {
                Mesh.CreateRectangle(),
                Mesh.FromObj("Assets/teapot.obj"),
                new Mesh(() => {
                    SphereGenerator.GenerateSphere(1.0f, 15, 15, out Vertex[] verts, out uint[] indices);
                    return (verts, indices);
                })
            };

            lightMesh = new Mesh(() =>
            {
                SphereGenerator.GenerateSphere(1.0f, 15, 15, out Vertex[] verts, out uint[] indices);
                return (verts, indices);
            });

            int counter = 0;
            foreach (var mesh in meshes)
            {
                mesh.Position = new Vector3(counter * 5, 0, 0);
                counter++;
            }

            gl.Enable(EnableCap.DepthTest);
            Loaded = true;
        }

        float totalTime = 0.0f;
        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            totalTime += dt;
            var options = renderOptions ?? RenderOptions.Default;
            options.Material = mat;


            meshes.Sort((o1, o2) => {
                var od1 = (int)Vector3.Distance(options.Camera.Position, o1.Position);
                var od2 = (int)Vector3.Distance(options.Camera.Position, o2.Position);

                return od1 > od2 ? 0 : 1;
            });

            options.Material.Shader.Use();
            for (int i = 0; i < lightColors.Length; i++)
            {
                var newPos = lightPositions[i] + new Vector3(MathF.Sin(totalTime * 2.0f) * 50, 0.0f, MathF.Cos(totalTime * 2.0f) * 50);

                options.Material.Shader.SetUniform("lightPositions[" + i + "]", newPos);
                options.Material.Shader.SetUniform("lightColors[" + i + "]", lightColors[i]);

                lightMesh.Position = newPos;
                lightMesh.Draw(RenderOptions.Default with { Camera = options.Camera });
            }

            foreach (var m in meshes)
                m.Draw(options);          
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

            ImGui.DragFloat("Metallicness", ref mat.MaterialDescription.Metallicness, 0.01f, 0.0f, 1.0f);
            ImGui.DragFloat("Roughness", ref mat.MaterialDescription.Roughness, 0.01f, 0.0f, 1.0f);
            ImGui.DragFloat("AO", ref mat.MaterialDescription.AmbientOcclusion, 0.01f, 0.0f, 1.0f);
        }
    }
}

