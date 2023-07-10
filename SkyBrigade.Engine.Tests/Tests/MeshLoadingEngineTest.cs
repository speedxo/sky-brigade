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
using System.Linq;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class MeshLoadingEngineTest : IEngineTest
    {
        public bool Loaded { get; set; } = false;
        public string Name { get; set; } = "Mesh Loading Test";

        private Vector3 scale = Vector3.One;
        private Vector3 rot = Vector3.Zero;

        private List<Mesh> meshes;
        private float gamma = 2.2f;
        // lights
        Vector3[] lightPositions = new [] {
            new Vector3(-10.0f,  10.0f, 10.0f),
            new Vector3( 10.0f,  10.0f, 10.0f),
            new Vector3(-10.0f, -10.0f, 10.0f),
            new Vector3( 10.0f, -10.0f, 10.0f)
        };

        Vector3[] lightColors;
    
        private Mesh lightMesh;

        Random rand = new Random();
        public void LoadContent(GL gl)
        {
            if (Loaded) return;


            // make some random vector3
            int intensity = 1000;
            lightColors = new [] {
                new Vector3(intensity),
                new Vector3(intensity),
                new Vector3(intensity),
                new Vector3(intensity),
            };

            //  now this is an intresting way to do this
            var materials = new AdvancedMaterial[] {
                AdvancedMaterial.LoadFromZip("Assets/pbr_textures/metal_ball.material"),
                AdvancedMaterial.LoadFromZip("Assets/pbr_textures/bricks_mortar.material"),
                AdvancedMaterial.LoadFromZip("Assets/pbr_textures/black_tiles.material"),
            };


            // dynamically create a mesh for each material, assinging the material to the mesh
            meshes = new List<Mesh>(materials.Length);
            for (int i = 0; i < materials.Length; i++)
                meshes.Add(Mesh.CreateSphere(1, 50, materials[i]));
            

            lightMesh = Mesh.CreateSphere(2, 5);

            int counter = 0;
            //meshes[2] = Mesh.FromObj("Assets/teapot.obj");
            foreach (var mesh in meshes)
            {
                mesh.Position = new Vector3(counter * 5, 0, 0);
                counter++;
            }

            gl.Enable(EnableCap.CullFace);
            gl.Enable(EnableCap.DepthTest);
            Loaded = true;
        }

        float totalTime = 0.0f;
        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            // Accumulate the time
            totalTime += dt;
            // Get the render options
            var options = renderOptions ?? RenderOptions.Default;

            meshes.Sort((o1, o2) => {
                if (Vector3.Distance(options.Camera.Position, o1.Position) < Vector3.Distance(options.Camera.Position, o2.Position)) return 1;
                return 0;
            });

            foreach (var item in meshes)
            {
                item.Use();
                item.SetUniform("uGamma", gamma);

                for (int v = 0; v < lightColors.Length; v++)
                {
                    var newPos = lightPositions[v] + new Vector3(MathF.Sin(totalTime * 2.0f) * 100, 0.0f, MathF.Cos(totalTime * 2.0f) * 100);

                    item.SetUniform("lightPositions[" + v + "]", newPos);
                    item.SetUniform("lightColors[" + v + "]", lightColors[v]);

                    lightMesh.Position = newPos;
                    lightMesh.Draw(RenderOptions.Default with { Camera = options.Camera });
                }


                item.Draw(options);
            }
        }

        // This method is called every frame.
        public void Update(float dt)
        {
            // Loop through all the meshes in the meshes array.
            foreach (var mesh in meshes)
            {
                // Check if the mesh's scale matches the scale instance variable.
                if (mesh.Scale != scale)
                    // If it doesn't, set the mesh's scale to the scale instance variable.
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
            ImGui.DragFloat("Gamma", ref gamma, 0.1f);

            if (ImGui.Button("Reset Lights"))
            {
                lightColors = new[] {
                new Vector3(rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f),
                new Vector3(rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f),
                new Vector3(rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f),
                new Vector3(rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f, rand.NextSingle() * 100.0f)
            };
            }
        }
    }
}

