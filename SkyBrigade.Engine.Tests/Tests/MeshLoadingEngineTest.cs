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

        Vector3[] lightColors = new []{
        new Vector3(300.0f, 300.0f, 300.0f),
        new Vector3(300.0f, 300.0f, 300.0f),
        new Vector3(300.0f, 300.0f, 300.0f),
        new Vector3(300.0f, 300.0f, 300.0f) 
    };
    
        private AdvancedMaterial[] materials;
        private Mesh lightMesh;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            //  now this is an intresting way to do this
            materials = new AdvancedMaterial[] {
                AdvancedMaterial.LoadFromDirectory("Assets/pbr_textures/metal_ball"),
                AdvancedMaterial.LoadFromDirectory("Assets/pbr_textures/bricks_mortar")
            };


            // dynamically create a mesh for each material
            meshes = (from mat in materials
                      select Mesh.CreateSphere(2.0f, 500)).ToList();

            lightMesh = Mesh.CreateSphere(2, 5);

            int counter = 0;
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
            
            // loop through each material and draw the corresposinding mesh using the material

            for (int i = 0; i < materials.Length; i++)
            {
                // Set the material
                options.Material = materials[i];

                options.Material.Shader.Use();
                options.Material.Shader.SetUniform("uGamma", gamma);
                for (int v = 0; v < lightColors.Length; v++)
                {
                    var newPos = lightPositions[v] + new Vector3(MathF.Sin(totalTime * 2.0f) * 10, 0.0f, MathF.Cos(totalTime * 2.0f) * 10);

                    options.Material.Shader.SetUniform("lightPositions[" + v + "]", newPos);
                    options.Material.Shader.SetUniform("lightColors[" + v + "]", lightColors[v]);

                    lightMesh.Position = newPos;
                    lightMesh.Draw(RenderOptions.Default with { Camera = options.Camera });
                }
                // Draw the mesh
                meshes[i].Draw(options);
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
        }
    }
}

